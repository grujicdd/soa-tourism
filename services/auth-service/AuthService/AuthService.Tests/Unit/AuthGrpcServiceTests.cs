using AuthService.Models;
using AuthService.Protos;
using AuthService.Services;
using FluentAssertions;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;

namespace AuthService.Tests.Unit;

public class AuthGrpcServiceTests
{
    private readonly Mock<IMongoCollection<User>> _usersCollectionMock;
    private readonly Mock<ILogger<AuthGrpcService>> _loggerMock;
    private readonly TokenService _tokenService;
    private readonly AuthGrpcService _service;
    private readonly JwtSettings _jwtSettings;

    public AuthGrpcServiceTests()
    {
        _usersCollectionMock = new Mock<IMongoCollection<User>>();
        _loggerMock = new Mock<ILogger<AuthGrpcService>>();

        _jwtSettings = new JwtSettings
        {
            Secret = "this-is-a-super-secret-key-for-testing-purposes-123!",
            ExpiryHours = 1,
            Issuer = "AuthService",
            Audience = "TourismApp"
        };

        _tokenService = new TokenService(_jwtSettings);
        _service = new AuthGrpcService(
            _usersCollectionMock.Object,
            _tokenService,
            _loggerMock.Object);
    }

    // ── Helper ───────────────────────────────────────────────────────────────

    private static User CreateTestUser(string role = "tourist") => new()
    {
        Id = "507f1f77bcf86cd799439011",
        Username = "testuser",
        Email = "test@example.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
        Role = role,
        IsBlocked = false,
        Profile = new UserProfileData
        {
            FirstName = "Test",
            LastName = "User",
            Bio = "A test user",
            Motto = "Testing is fun"
        }
    };

    private static ServerCallContext CreateContext() =>
        TestServerCallContext.Create(
            method: "test",
            host: "localhost",
            deadline: DateTime.UtcNow.AddMinutes(1),
            requestHeaders: new Metadata(),
            cancellationToken: CancellationToken.None,
            peer: "localhost",
            authContext: null,
            contextPropagationToken: null,
            writeHeadersFunc: _ => Task.CompletedTask,
            writeOptionsGetter: () => WriteOptions.Default,
            writeOptionsSetter: _ => { });

    private void SetupFindReturnsUser(User? user)
    {
        var cursorMock = new Mock<IAsyncCursor<User>>();
        cursorMock
            .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(user != null)
            .ReturnsAsync(false);
        cursorMock
            .Setup(c => c.Current)
            .Returns(user != null ? new[] { user } : Array.Empty<User>());

        _usersCollectionMock
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<FindOptions<User, User>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorMock.Object);
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_InvalidRole_ReturnsFailure()
    {
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "new@example.com",
            Password = "Password123!",
            Role = "superadmin"
        };

        var result = await _service.Register(request, CreateContext());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid role");
    }

    [Fact]
    public async Task Register_DuplicateUsername_ReturnsFailure()
    {
        SetupFindReturnsUser(CreateTestUser());

        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "different@example.com",
            Password = "Password123!",
            Role = "tourist"
        };

        var result = await _service.Register(request, CreateContext());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Username already exists");
    }

    [Fact]
    public async Task Register_ValidGuideRole_ReturnsSuccess()
    {
        SetupFindReturnsUser(null);

        _usersCollectionMock
            .Setup(c => c.InsertOneAsync(
                It.IsAny<User>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new RegisterRequest
        {
            Username = "newguide",
            Email = "guide@example.com",
            Password = "Password123!",
            Role = "guide"
        };

        var result = await _service.Register(request, CreateContext());

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Register_ValidTouristRole_ReturnsSuccess()
    {
        SetupFindReturnsUser(null);

        _usersCollectionMock
            .Setup(c => c.InsertOneAsync(
                It.IsAny<User>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new RegisterRequest
        {
            Username = "newtourist",
            Email = "tourist@example.com",
            Password = "Password123!",
            Role = "tourist"
        };

        var result = await _service.Register(request, CreateContext());

        result.Success.Should().BeTrue();
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_UserNotFound_ReturnsFailure()
    {
        SetupFindReturnsUser(null);

        var request = new LoginRequest
        {
            Username = "nobody",
            Password = "Password123!"
        };

        var result = await _service.Login(request, CreateContext());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid username or password");
    }

    [Fact]
    public async Task Login_BlockedUser_ReturnsFailure()
    {
        var blockedUser = CreateTestUser();
        blockedUser.IsBlocked = true;
        SetupFindReturnsUser(blockedUser);

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "Password123!"
        };

        var result = await _service.Login(request, CreateContext());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("blocked");
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsFailure()
    {
        SetupFindReturnsUser(CreateTestUser());

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "WrongPassword!"
        };

        var result = await _service.Login(request, CreateContext());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid username or password");
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsSuccess()
    {
        SetupFindReturnsUser(CreateTestUser());

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "Password123!"
        };

        var result = await _service.Login(request, CreateContext());

        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsCorrectRole()
    {
        SetupFindReturnsUser(CreateTestUser("guide"));

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "Password123!"
        };

        var result = await _service.Login(request, CreateContext());

        result.Role.Should().Be("guide");
    }

    // ── ValidateToken ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ValidateToken_ValidToken_ReturnsValid()
    {
        var user = CreateTestUser();
        var token = _tokenService.GenerateToken(user);

        var request = new ValidateTokenRequest { Token = token };
        var result = await _service.ValidateToken(request, CreateContext());

        result.Valid.Should().BeTrue();
        result.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task ValidateToken_InvalidToken_ReturnsInvalid()
    {
        var request = new ValidateTokenRequest { Token = "garbage.token.here" };
        var result = await _service.ValidateToken(request, CreateContext());

        result.Valid.Should().BeFalse();
    }
}
