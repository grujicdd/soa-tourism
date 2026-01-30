using Gateway.DTOs;
using Gateway.GrpcClients;
using Microsoft.AspNetCore.Mvc;
using BlogService.Protos;

namespace Gateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogController : ControllerBase
{
    private readonly BlogGrpcClient _blogClient;
    private readonly ILogger<BlogController> _logger;

    public BlogController(BlogGrpcClient blogClient, ILogger<BlogController> logger)
    {
        _blogClient = blogClient;
        _logger = logger;
    }

    // ============ Blog Operations ============

    [HttpPost]
    public async Task<ActionResult<BlogResponseDto>> CreateBlog([FromBody] CreateBlogRequestDto request)
    {
        try
        {
            var grpcRequest = new CreateBlogRequest
            {
                UserId = request.UserId,
                Username = request.Username,
                Title = request.Title,
                Description = request.Description,
                Images = { request.Images }
            };

            var response = await _blogClient.Client.CreateBlogAsync(grpcRequest);

            return Ok(new BlogResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Blog = response.Blog != null ? MapBlogToDto(response.Blog) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating blog: {ex.Message}");
            return StatusCode(500, new BlogResponseDto
            {
                Success = false,
                Message = "An error occurred while creating blog."
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<BlogsResponseDto>> GetBlogs([FromQuery] string? userId = null, [FromQuery] int skip = 0, [FromQuery] int limit = 20)
    {
        try
        {
            var grpcRequest = new GetBlogsRequest
            {
                UserId = userId ?? string.Empty,
                Skip = skip,
                Limit = limit
            };

            var response = await _blogClient.Client.GetBlogsAsync(grpcRequest);

            return Ok(new BlogsResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Blogs = response.Blogs.Select(MapBlogToDto).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting blogs: {ex.Message}");
            return StatusCode(500, new BlogsResponseDto
            {
                Success = false,
                Message = "An error occurred while getting blogs."
            });
        }
    }

    [HttpGet("{blogId}")]
    public async Task<ActionResult<BlogResponseDto>> GetBlogById(string blogId)
    {
        try
        {
            var grpcRequest = new GetBlogByIdRequest
            {
                BlogId = blogId
            };

            var response = await _blogClient.Client.GetBlogByIdAsync(grpcRequest);

            if (!response.Success)
            {
                return NotFound(new BlogResponseDto
                {
                    Success = false,
                    Message = response.Message
                });
            }

            return Ok(new BlogResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Blog = response.Blog != null ? MapBlogToDto(response.Blog) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting blog: {ex.Message}");
            return StatusCode(500, new BlogResponseDto
            {
                Success = false,
                Message = "An error occurred while getting blog."
            });
        }
    }

    // ============ Comment Operations ============

    [HttpPost("{blogId}/comments")]
    public async Task<ActionResult<CommentResponseDto>> AddComment(string blogId, [FromBody] AddCommentRequestDto request)
    {
        try
        {
            var grpcRequest = new AddCommentRequest
            {
                BlogId = blogId,
                UserId = request.UserId,
                Username = request.Username,
                Text = request.Text
            };

            var response = await _blogClient.Client.AddCommentAsync(grpcRequest);

            return Ok(new CommentResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Comment = response.Comment != null ? MapCommentToDto(response.Comment) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding comment: {ex.Message}");
            return StatusCode(500, new CommentResponseDto
            {
                Success = false,
                Message = "An error occurred while adding comment."
            });
        }
    }

    [HttpGet("{blogId}/comments")]
    public async Task<ActionResult<CommentsResponseDto>> GetComments(string blogId)
    {
        try
        {
            var grpcRequest = new GetCommentsRequest
            {
                BlogId = blogId
            };

            var response = await _blogClient.Client.GetCommentsAsync(grpcRequest);

            return Ok(new CommentsResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Comments = response.Comments.Select(MapCommentToDto).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting comments: {ex.Message}");
            return StatusCode(500, new CommentsResponseDto
            {
                Success = false,
                Message = "An error occurred while getting comments."
            });
        }
    }

    // ============ Like Operations ============

    [HttpPost("{blogId}/like")]
    public async Task<ActionResult<LikeResponseDto>> LikeBlog(string blogId, [FromBody] string userId)
    {
        try
        {
            var grpcRequest = new LikeBlogRequest
            {
                BlogId = blogId,
                UserId = userId
            };

            var response = await _blogClient.Client.LikeBlogAsync(grpcRequest);

            return Ok(new LikeResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                LikeCount = response.LikeCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error liking blog: {ex.Message}");
            return StatusCode(500, new LikeResponseDto
            {
                Success = false,
                Message = "An error occurred while liking blog."
            });
        }
    }

    [HttpDelete("{blogId}/like")]
    public async Task<ActionResult<LikeResponseDto>> UnlikeBlog(string blogId, [FromBody] string userId)
    {
        try
        {
            var grpcRequest = new UnlikeBlogRequest
            {
                BlogId = blogId,
                UserId = userId
            };

            var response = await _blogClient.Client.UnlikeBlogAsync(grpcRequest);

            return Ok(new LikeResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                LikeCount = response.LikeCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error unliking blog: {ex.Message}");
            return StatusCode(500, new LikeResponseDto
            {
                Success = false,
                Message = "An error occurred while unliking blog."
            });
        }
    }

    // ============ Follow Operations ============

    [HttpPost("follow/{followerId}")]
    public async Task<ActionResult<FollowResponseDto>> FollowUser(string followerId, [FromBody] FollowUserRequestDto request)
    {
        try
        {
            var grpcRequest = new FollowUserRequest
            {
                FollowerId = followerId,
                FollowerUsername = request.FollowerUsername,
                FolloweeId = request.FolloweeId,
                FolloweeUsername = request.FolloweeUsername
            };

            var response = await _blogClient.Client.FollowUserAsync(grpcRequest);

            return Ok(new FollowResponseDto
            {
                Success = response.Success,
                Message = response.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error following user: {ex.Message}");
            return StatusCode(500, new FollowResponseDto
            {
                Success = false,
                Message = "An error occurred while following user."
            });
        }
    }

    [HttpDelete("unfollow/{followerId}/{followeeId}")]
    public async Task<ActionResult<FollowResponseDto>> UnfollowUser(string followerId, string followeeId)
    {
        try
        {
            var grpcRequest = new UnfollowUserRequest
            {
                FollowerId = followerId,
                FolloweeId = followeeId
            };

            var response = await _blogClient.Client.UnfollowUserAsync(grpcRequest);

            return Ok(new FollowResponseDto
            {
                Success = response.Success,
                Message = response.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error unfollowing user: {ex.Message}");
            return StatusCode(500, new FollowResponseDto
            {
                Success = false,
                Message = "An error occurred while unfollowing user."
            });
        }
    }

    [HttpGet("following/{userId}")]
    public async Task<ActionResult<FollowersResponseDto>> GetFollowing(string userId)
    {
        try
        {
            var grpcRequest = new GetFollowingRequest
            {
                UserId = userId
            };

            var response = await _blogClient.Client.GetFollowingAsync(grpcRequest);

            return Ok(new FollowersResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Users = response.Users.Select(u => new UserInfoDto
                {
                    UserId = u.UserId,
                    Username = u.Username
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting following: {ex.Message}");
            return StatusCode(500, new FollowersResponseDto
            {
                Success = false,
                Message = "An error occurred while getting following."
            });
        }
    }

    [HttpGet("followers/{userId}")]
    public async Task<ActionResult<FollowersResponseDto>> GetFollowers(string userId)
    {
        try
        {
            var grpcRequest = new GetFollowersRequest
            {
                UserId = userId
            };

            var response = await _blogClient.Client.GetFollowersAsync(grpcRequest);

            return Ok(new FollowersResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Users = response.Users.Select(u => new UserInfoDto
                {
                    UserId = u.UserId,
                    Username = u.Username
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting followers: {ex.Message}");
            return StatusCode(500, new FollowersResponseDto
            {
                Success = false,
                Message = "An error occurred while getting followers."
            });
        }
    }

    [HttpGet("recommendations/{userId}")]
    public async Task<ActionResult<RecommendationsResponseDto>> GetRecommendations(string userId, [FromQuery] int limit = 5)
    {
        try
        {
            var grpcRequest = new GetRecommendationsRequest
            {
                UserId = userId,
                Limit = limit
            };

            var response = await _blogClient.Client.GetFollowRecommendationsAsync(grpcRequest);

            return Ok(new RecommendationsResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Recommendations = response.Recommendations.Select(r => new UserInfoDto
                {
                    UserId = r.UserId,
                    Username = r.Username
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting recommendations: {ex.Message}");
            return StatusCode(500, new RecommendationsResponseDto
            {
                Success = false,
                Message = "An error occurred while getting recommendations."
            });
        }
    }

    // ============ Helper Methods ============

    private static BlogPostDto MapBlogToDto(BlogPost blog)
    {
        return new BlogPostDto
        {
            Id = blog.Id,
            UserId = blog.UserId,
            Username = blog.Username,
            Title = blog.Title,
            Description = blog.Description,
            Images = blog.Images.ToList(),
            CreatedAt = blog.CreatedAt,
            LikeCount = blog.LikeCount,
            CommentCount = blog.CommentCount
        };
    }

    private static CommentDto MapCommentToDto(BlogService.Protos.Comment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            BlogId = comment.BlogId,
            UserId = comment.UserId,
            Username = comment.Username,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt,
            LastModified = comment.LastModified
        };
    }
}