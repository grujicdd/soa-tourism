namespace Gateway.DTOs;

// Blog Operations
public class CreateBlogRequestDto
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();
}

public class BlogPostDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();
    public string CreatedAt { get; set; } = string.Empty;
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
}

public class BlogResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public BlogPostDto? Blog { get; set; }
}

public class BlogsResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<BlogPostDto> Blogs { get; set; } = new();
}

// Comment Operations
public class AddCommentRequestDto
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class CommentDto
{
    public string Id { get; set; } = string.Empty;
    public string BlogId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string LastModified { get; set; } = string.Empty;
}

public class CommentResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public CommentDto? Comment { get; set; }
}

public class CommentsResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<CommentDto> Comments { get; set; } = new();
}

// Like Operations
public class LikeResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int LikeCount { get; set; }
}

// Follow Operations
public class FollowUserRequestDto
{
    public string FollowerUsername { get; set; } = string.Empty;
    public string FolloweeId { get; set; } = string.Empty;
    public string FolloweeUsername { get; set; } = string.Empty;
}

public class FollowResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class UserInfoDto
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}

public class FollowersResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<UserInfoDto> Users { get; set; } = new();
}

public class RecommendationsResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<UserInfoDto> Recommendations { get; set; } = new();
}
