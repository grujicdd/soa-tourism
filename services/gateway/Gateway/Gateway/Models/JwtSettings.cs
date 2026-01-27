namespace Gateway.Models;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "AuthService";
    public string Audience { get; set; } = "TourismApp";
}
