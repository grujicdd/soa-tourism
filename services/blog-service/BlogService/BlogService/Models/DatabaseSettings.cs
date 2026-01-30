namespace BlogService.Models;

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "blog-db";
    public string BlogsCollectionName { get; set; } = "blogs";
    public string CommentsCollectionName { get; set; } = "comments";
    public string LikesCollectionName { get; set; } = "likes";
}