using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlogService.Models;

public class Comment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("blogId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string BlogId { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("text")]
    public string Text { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("lastModified")]
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
