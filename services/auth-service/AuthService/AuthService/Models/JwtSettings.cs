namespace AuthService.Models;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public int ExpiryHours { get; set; } = 24;
    public string Issuer { get; set; } = "AuthService";
    public string Audience { get; set; } = "TourismApp";
}
