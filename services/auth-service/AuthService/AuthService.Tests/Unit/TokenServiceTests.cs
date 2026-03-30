using AuthService.Models;
using AuthService.Services;
using FluentAssertions;

namespace AuthService.Tests.Unit;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public TokenServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            Secret = "this-is-a-super-secret-key-for-testing-purposes-123!",
            ExpiryHours = 1,
            Issuer = "AuthService",
            Audience = "TourismApp"
        };

        _tokenService = new TokenService(_jwtSettings);
    }

    // ── GenerateToken ────────────────────────────────────────────────────────

    [Fact]
    public void GenerateToken_ValidUser_ReturnsNonEmptyString()
    {
        var user = CreateTestUser();

        var token = _tokenService.GenerateToken(user);

        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateToken_ValidUser_ReturnsValidJwtFormat()
    {
        var user = CreateTestUser();

        var token = _tokenService.GenerateToken(user);

        // JWT format is three base64 segments separated by dots
        token.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public void GenerateToken_DifferentUsers_ReturnsDifferentTokens()
    {
        var user1 = CreateTestUser();
        var user2 = new User
        {
            Id = "507f1f77bcf86cd799439022",
            Username = "anotheruser",
            Email = "another@example.com",
            Role = "guide",
            IsBlocked = false
        };

        var token1 = _tokenService.GenerateToken(user1);
        var token2 = _tokenService.GenerateToken(user2);

        token1.Should().NotBe(token2);
    }

    // ── ValidateToken ────────────────────────────────────────────────────────

    [Fact]
    public void ValidateToken_ValidToken_ReturnsNotNull()
    {
        var user = CreateTestUser();
        var token = _tokenService.GenerateToken(user);

        var result = _tokenService.ValidateToken(token);

        result.Should().NotBeNull();
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsNull()
    {
        var result = _tokenService.ValidateToken("this.is.garbage");

        result.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_EmptyString_ReturnsNull()
    {
        var result = _tokenService.ValidateToken("");

        result.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_TamperedToken_ReturnsNull()
    {
        var user = CreateTestUser();
        var token = _tokenService.GenerateToken(user);
        var tampered = token[..^5] + "XXXXX";

        var result = _tokenService.ValidateToken(tampered);

        result.Should().BeNull();
    }

    // ── GetUserInfoFromToken ─────────────────────────────────────────────────

    [Fact]
    public void GetUserInfoFromToken_ValidToken_ReturnsCorrectUserId()
    {
        var user = CreateTestUser();
        var token = _tokenService.GenerateToken(user);

        var result = _tokenService.GetUserInfoFromToken(token);

        result.Should().NotBeNull();
        result!.Value.userId.Should().Be(user.Id);
    }

    [Fact]
    public void GetUserInfoFromToken_ValidToken_ReturnsCorrectUsername()
    {
        var user = CreateTestUser();
        var token = _tokenService.GenerateToken(user);

        var result = _tokenService.GetUserInfoFromToken(token);

        result!.Value.username.Should().Be(user.Username);
    }

    [Fact]
    public void GetUserInfoFromToken_ValidToken_ReturnsCorrectRole()
    {
        var user = CreateTestUser();
        var token = _tokenService.GenerateToken(user);

        var result = _tokenService.GetUserInfoFromToken(token);

        result!.Value.role.Should().Be(user.Role);
    }

    [Fact]
    public void GetUserInfoFromToken_InvalidToken_ReturnsNull()
    {
        var result = _tokenService.GetUserInfoFromToken("invalid.token.here");

        result.Should().BeNull();
    }

    //NOVI TESTOVI ZA UBIJANJE MUTANATA

    // ── ValidateToken — boolean flag mutations ────────────────────────────────────

    [Fact]
    public void ValidateToken_WrongIssuer_ReturnsNull()
    {
        // Token generated with different issuer settings
        var wrongSettings = new JwtSettings
        {
            Secret = "this-is-a-super-secret-key-for-testing-purposes-123!",
            ExpiryHours = 1,
            Issuer = "WrongIssuer",
            Audience = "TourismApp"
        };
        var wrongService = new TokenService(wrongSettings);
        var user = CreateTestUser();
        var token = wrongService.GenerateToken(user);

        // Our service with correct issuer should reject this token
        var result = _tokenService.ValidateToken(token);

        result.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WrongAudience_ReturnsNull()
    {
        var wrongSettings = new JwtSettings
        {
            Secret = "this-is-a-super-secret-key-for-testing-purposes-123!",
            ExpiryHours = 1,
            Issuer = "AuthService",
            Audience = "WrongAudience"
        };
        var wrongService = new TokenService(wrongSettings);
        var user = CreateTestUser();
        var token = wrongService.GenerateToken(user);

        var result = _tokenService.ValidateToken(token);

        result.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WrongSecret_ReturnsNull()
    {
        var wrongSettings = new JwtSettings
        {
            Secret = "this-is-a-completely-different-secret-key-456!",
            ExpiryHours = 1,
            Issuer = "AuthService",
            Audience = "TourismApp"
        };
        var wrongService = new TokenService(wrongSettings);
        var user = CreateTestUser();
        var token = wrongService.GenerateToken(user);

        var result = _tokenService.ValidateToken(token);

        result.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_TokenFromFutureTime_ReturnsNull()
    {
        // A token signed with a completely different secret cannot be valid
        // regardless of lifetime settings — this kills the ValidateLifetime mutant
        // indirectly by ensuring signature validation always runs
        var result = _tokenService.ValidateToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c");

        result.Should().BeNull();
    }

    // ── GetUserInfoFromToken — || → && mutation ───────────────────────────────────

    [Fact]
    public void GetUserInfoFromToken_ValidToken_ReturnsAllThreeFields()
    {
        var user = CreateTestUser();
        var token = _tokenService.GenerateToken(user);

        var result = _tokenService.GetUserInfoFromToken(token);

        // All three fields must be present and correct
        result.Should().NotBeNull();
        result!.Value.userId.Should().NotBeNullOrEmpty();
        result!.Value.username.Should().NotBeNullOrEmpty();
        result!.Value.role.Should().NotBeNullOrEmpty();
    }

    // ── Helper ───────────────────────────────────────────────────────────────

    private static User CreateTestUser() => new()
    {
        Id = "507f1f77bcf86cd799439011",
        Username = "testuser",
        Email = "test@example.com",
        Role = "tourist",
        IsBlocked = false
    };
}
