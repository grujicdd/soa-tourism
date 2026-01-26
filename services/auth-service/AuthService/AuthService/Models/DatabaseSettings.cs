namespace AuthService.Models;

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "auth-db";
    public string UsersCollectionName { get; set; } = "users";
}
