using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthService.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("role")]
    public string Role { get; set; } = string.Empty; // "admin", "guide", "tourist"

    [BsonElement("isBlocked")]
    public bool IsBlocked { get; set; } = false;

    [BsonElement("profile")]
    public UserProfileData Profile { get; set; } = new UserProfileData();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class UserProfileData
{
    [BsonElement("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [BsonElement("lastName")]
    public string LastName { get; set; } = string.Empty;

    [BsonElement("profilePicture")]
    public string ProfilePicture { get; set; } = string.Empty;

    [BsonElement("bio")]
    public string Bio { get; set; } = string.Empty;

    [BsonElement("motto")]
    public string Motto { get; set; } = string.Empty;
}
