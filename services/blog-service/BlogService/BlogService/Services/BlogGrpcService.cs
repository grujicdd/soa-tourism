using BlogService.Models;
using BlogService.Neo4jClient;
using BlogService.Protos;
using Grpc.Core;
using MongoDB.Driver;

namespace BlogService.Services;

public class BlogGrpcService : BlogService.Protos.BlogService.BlogServiceBase
{
    private readonly IMongoCollection<Blog> _blogsCollection;
    private readonly IMongoCollection<Models.Comment> _commentsCollection;
    private readonly IMongoCollection<Like> _likesCollection;
    private readonly FollowerService _followerService;
    private readonly ILogger<BlogGrpcService> _logger;

    public BlogGrpcService(
        IMongoCollection<Blog> blogsCollection,
        IMongoCollection<Models.Comment> commentsCollection,
        IMongoCollection<Like> likesCollection,
        FollowerService followerService,
        ILogger<BlogGrpcService> logger)
    {
        _blogsCollection = blogsCollection;
        _commentsCollection = commentsCollection;
        _likesCollection = likesCollection;
        _followerService = followerService;
        _logger = logger;
    }

    // ============ Blog Operations ============

    public override async Task<BlogResponse> CreateBlog(CreateBlogRequest request, ServerCallContext context)
    {
        try
        {
            var blog = new Blog
            {
                UserId = request.UserId,
                Username = request.Username,
                Title = request.Title,
                Description = request.Description,
                Images = request.Images.ToList(),
                CreatedAt = DateTime.UtcNow,
                LikeCount = 0,
                CommentCount = 0
            };

            await _blogsCollection.InsertOneAsync(blog);

            _logger.LogInformation($"Blog created by {blog.Username}: {blog.Title}");

            return new BlogResponse
            {
                Success = true,
                Message = "Blog created successfully",
                Blog = MapBlogToProto(blog)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating blog: {ex.Message}");
            return new BlogResponse
            {
                Success = false,
                Message = "Failed to create blog"
            };
        }
    }

    public override async Task<BlogsResponse> GetBlogs(GetBlogsRequest request, ServerCallContext context)
    {
        try
        {
            List<Blog> blogs;

            if (!string.IsNullOrEmpty(request.UserId))
            {
                // Get blogs from users that this user follows
                var following = await _followerService.GetFollowing(request.UserId);
                var followingIds = following.Select(f => f.userId).ToList();
                followingIds.Add(request.UserId); // Include own blogs

                blogs = await _blogsCollection
                    .Find(b => followingIds.Contains(b.UserId))
                    .SortByDescending(b => b.CreatedAt)
                    .Skip(request.Skip)
                    .Limit(request.Limit > 0 ? request.Limit : 20)
                    .ToListAsync();
            }
            else
            {
                // Get all blogs
                blogs = await _blogsCollection
                    .Find(_ => true)
                    .SortByDescending(b => b.CreatedAt)
                    .Skip(request.Skip)
                    .Limit(request.Limit > 0 ? request.Limit : 20)
                    .ToListAsync();
            }

            return new BlogsResponse
            {
                Success = true,
                Message = "Blogs retrieved successfully",
                Blogs = { blogs.Select(MapBlogToProto) }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting blogs: {ex.Message}");
            return new BlogsResponse
            {
                Success = false,
                Message = "Failed to get blogs"
            };
        }
    }

    public override async Task<BlogResponse> GetBlogById(GetBlogByIdRequest request, ServerCallContext context)
    {
        try
        {
            var blog = await _blogsCollection
                .Find(b => b.Id == request.BlogId)
                .FirstOrDefaultAsync();

            if (blog == null)
            {
                return new BlogResponse
                {
                    Success = false,
                    Message = "Blog not found"
                };
            }

            return new BlogResponse
            {
                Success = true,
                Message = "Blog retrieved successfully",
                Blog = MapBlogToProto(blog)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting blog: {ex.Message}");
            return new BlogResponse
            {
                Success = false,
                Message = "Failed to get blog"
            };
        }
    }

    // ============ Comment Operations ============

    public override async Task<CommentResponse> AddComment(AddCommentRequest request, ServerCallContext context)
    {
        try
        {
            // Get the blog to find the blog author
            var blog = await _blogsCollection
                .Find(b => b.Id == request.BlogId)
                .FirstOrDefaultAsync();

            if (blog == null)
            {
                return new CommentResponse
                {
                    Success = false,
                    Message = "Blog not found"
                };
            }

            // Check if user is following the blog author (unless commenting on own blog)
            if (blog.UserId != request.UserId)
            {
                var isFollowing = await _followerService.IsFollowing(request.UserId, blog.UserId);
                if (!isFollowing)
                {
                    return new CommentResponse
                    {
                        Success = false,
                        Message = "You must follow the user to comment on their blog"
                    };
                }
            }

            var comment = new Models.Comment
            {
                BlogId = request.BlogId,
                UserId = request.UserId,
                Username = request.Username,
                Text = request.Text,
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            await _commentsCollection.InsertOneAsync(comment);

            // Update comment count on blog
            await _blogsCollection.UpdateOneAsync(
                b => b.Id == request.BlogId,
                Builders<Blog>.Update.Inc(b => b.CommentCount, 1)
            );

            _logger.LogInformation($"Comment added by {comment.Username} on blog {request.BlogId}");

            return new CommentResponse
            {
                Success = true,
                Message = "Comment added successfully",
                Comment = MapCommentToProto(comment)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding comment: {ex.Message}");
            return new CommentResponse
            {
                Success = false,
                Message = "Failed to add comment"
            };
        }
    }

    public override async Task<CommentsResponse> GetComments(GetCommentsRequest request, ServerCallContext context)
    {
        try
        {
            var comments = await _commentsCollection
                .Find(c => c.BlogId == request.BlogId)
                .SortBy(c => c.CreatedAt)
                .ToListAsync();

            return new CommentsResponse
            {
                Success = true,
                Message = "Comments retrieved successfully",
                Comments = { comments.Select(MapCommentToProto) }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting comments: {ex.Message}");
            return new CommentsResponse
            {
                Success = false,
                Message = "Failed to get comments"
            };
        }
    }

    // ============ Like Operations ============

    public override async Task<LikeResponse> LikeBlog(LikeBlogRequest request, ServerCallContext context)
    {
        try
        {
            // Check if already liked
            var existingLike = await _likesCollection
                .Find(l => l.BlogId == request.BlogId && l.UserId == request.UserId)
                .FirstOrDefaultAsync();

            if (existingLike != null)
            {
                return new LikeResponse
                {
                    Success = false,
                    Message = "You already liked this blog"
                };
            }

            var like = new Like
            {
                BlogId = request.BlogId,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _likesCollection.InsertOneAsync(like);

            // Update like count on blog
            await _blogsCollection.UpdateOneAsync(
                b => b.Id == request.BlogId,
                Builders<Blog>.Update.Inc(b => b.LikeCount, 1)
            );

            var blog = await _blogsCollection
                .Find(b => b.Id == request.BlogId)
                .FirstOrDefaultAsync();

            return new LikeResponse
            {
                Success = true,
                Message = "Blog liked successfully",
                LikeCount = blog?.LikeCount ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error liking blog: {ex.Message}");
            return new LikeResponse
            {
                Success = false,
                Message = "Failed to like blog"
            };
        }
    }

    public override async Task<LikeResponse> UnlikeBlog(UnlikeBlogRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _likesCollection.DeleteOneAsync(
                l => l.BlogId == request.BlogId && l.UserId == request.UserId
            );

            if (result.DeletedCount == 0)
            {
                return new LikeResponse
                {
                    Success = false,
                    Message = "Like not found"
                };
            }

            // Update like count on blog
            await _blogsCollection.UpdateOneAsync(
                b => b.Id == request.BlogId,
                Builders<Blog>.Update.Inc(b => b.LikeCount, -1)
            );

            var blog = await _blogsCollection
                .Find(b => b.Id == request.BlogId)
                .FirstOrDefaultAsync();

            return new LikeResponse
            {
                Success = true,
                Message = "Blog unliked successfully",
                LikeCount = blog?.LikeCount ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error unliking blog: {ex.Message}");
            return new LikeResponse
            {
                Success = false,
                Message = "Failed to unlike blog"
            };
        }
    }

    // ============ Follow Operations ============

    public override async Task<FollowResponse> FollowUser(FollowUserRequest request, ServerCallContext context)
    {
        try
        {
            if (request.FollowerId == request.FolloweeId)
            {
                return new FollowResponse
                {
                    Success = false,
                    Message = "Cannot follow yourself"
                };
            }

            var success = await _followerService.FollowUser(
                request.FollowerId,
                request.FollowerUsername,
                request.FolloweeId,
                request.FolloweeUsername
            );

            if (!success)
            {
                return new FollowResponse
                {
                    Success = false,
                    Message = "Failed to follow user"
                };
            }

            _logger.LogInformation($"{request.FollowerUsername} followed {request.FolloweeUsername}");

            return new FollowResponse
            {
                Success = true,
                Message = "User followed successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error following user: {ex.Message}");
            return new FollowResponse
            {
                Success = false,
                Message = "Failed to follow user"
            };
        }
    }

    public override async Task<FollowResponse> UnfollowUser(UnfollowUserRequest request, ServerCallContext context)
    {
        try
        {
            var success = await _followerService.UnfollowUser(request.FollowerId, request.FolloweeId);

            if (!success)
            {
                return new FollowResponse
                {
                    Success = false,
                    Message = "Failed to unfollow user"
                };
            }

            return new FollowResponse
            {
                Success = true,
                Message = "User unfollowed successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error unfollowing user: {ex.Message}");
            return new FollowResponse
            {
                Success = false,
                Message = "Failed to unfollow user"
            };
        }
    }

    public override async Task<IsFollowingResponse> IsFollowing(IsFollowingRequest request, ServerCallContext context)
    {
        try
        {
            var isFollowing = await _followerService.IsFollowing(request.FollowerId, request.FolloweeId);

            return new IsFollowingResponse
            {
                IsFollowing = isFollowing
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking following status: {ex.Message}");
            return new IsFollowingResponse
            {
                IsFollowing = false
            };
        }
    }

    public override async Task<FollowersResponse> GetFollowers(GetFollowersRequest request, ServerCallContext context)
    {
        try
        {
            var followers = await _followerService.GetFollowers(request.UserId);

            return new FollowersResponse
            {
                Success = true,
                Message = "Followers retrieved successfully",
                Users = { followers.Select(f => new UserInfo { UserId = f.userId, Username = f.username }) }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting followers: {ex.Message}");
            return new FollowersResponse
            {
                Success = false,
                Message = "Failed to get followers"
            };
        }
    }

    public override async Task<FollowersResponse> GetFollowing(GetFollowingRequest request, ServerCallContext context)
    {
        try
        {
            var following = await _followerService.GetFollowing(request.UserId);

            return new FollowersResponse
            {
                Success = true,
                Message = "Following retrieved successfully",
                Users = { following.Select(f => new UserInfo { UserId = f.userId, Username = f.username }) }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting following: {ex.Message}");
            return new FollowersResponse
            {
                Success = false,
                Message = "Failed to get following"
            };
        }
    }

    public override async Task<RecommendationsResponse> GetFollowRecommendations(GetRecommendationsRequest request, ServerCallContext context)
    {
        try
        {
            var recommendations = await _followerService.GetRecommendations(
                request.UserId,
                request.Limit > 0 ? request.Limit : 5
            );

            return new RecommendationsResponse
            {
                Success = true,
                Message = "Recommendations retrieved successfully",
                Recommendations = { recommendations.Select(r => new UserInfo { UserId = r.userId, Username = r.username }) }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting recommendations: {ex.Message}");
            return new RecommendationsResponse
            {
                Success = false,
                Message = "Failed to get recommendations"
            };
        }
    }

    // ============ Helper Methods ============

    private static BlogPost MapBlogToProto(Blog blog)
    {
        return new BlogPost
        {
            Id = blog.Id,
            UserId = blog.UserId,
            Username = blog.Username,
            Title = blog.Title,
            Description = blog.Description,
            Images = { blog.Images },
            CreatedAt = blog.CreatedAt.ToString("o"),
            LikeCount = blog.LikeCount,
            CommentCount = blog.CommentCount
        };
    }

    private static Protos.Comment MapCommentToProto(Models.Comment comment)
    {
        return new Protos.Comment
        {
            Id = comment.Id,
            BlogId = comment.BlogId,
            UserId = comment.UserId,
            Username = comment.Username,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt.ToString("o"),
            LastModified = comment.LastModified.ToString("o")
        };
    }
}
