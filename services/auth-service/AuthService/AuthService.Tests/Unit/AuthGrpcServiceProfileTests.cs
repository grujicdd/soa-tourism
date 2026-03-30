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

public class AuthGrpcServiceProfileTests
{
    private readonly Mock<IMongoCollection<User>> _usersCollectionMock;
    private readonly Mock<ILogger<AuthGrpcService>> _loggerMock;
    private readonly TokenService _tokenService;
    private readonly AuthGrpcService _service;

    public AuthGrpcServiceProfileTests()
    {
        _usersCollectionMock = new Mock<IMongoCollection<User>>();
        _loggerMock = new Mock<ILogger<AuthGrpcService>>();

        var jwtSettings = new JwtSettings
        {
            Secret = "this-is-a-super-secret-key-for-testing-purposes-123!",
            ExpiryHours = 1,
            Issuer = "AuthService",
            Audience = "TourismApp"
        };

        _tokenService = new TokenService(jwtSettings);
        _service = new AuthGrpcService(
            _usersCollectionMock.Object,
            _tokenService,
            _loggerMock.Object);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static User CreateTestUser() => new()
    {
        Id = "507f1f77bcf86cd799439011",
        Username = "testuser",
        Email = "test@example.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
        Role = "tourist",
        IsBlocked = false,
        Profile = new UserProfileData
        {
            FirstName = "Test",
            LastName = "User",
            Bio = "A test user",
            Motto = "Testing is fun",
            ProfilePicture = ""
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

    private void SetupUpdateSucceeds()
    {
        _usersCollectionMock
            .Setup(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<UpdateDefinition<User>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));
    }

    // ── GetProfile ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProfile_UserExists_ReturnsSuccess()
    {
        SetupFindReturnsUser(CreateTestUser());

        var request = new GetProfileRequest { UserId = "507f1f77bcf86cd799439011" };
        var result = await _service.GetProfile(request, CreateContext());

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetProfile_UserExists_ReturnsCorrectUsername()
    {
        SetupFindReturnsUser(CreateTestUser());

        var request = new GetProfileRequest { UserId = "507f1f77bcf86cd799439011" };
        var result = await _service.GetProfile(request, CreateContext());

        result.Profile.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetProfile_UserExists_ReturnsCorrectEmail()
    {
        SetupFindReturnsUser(CreateTestUser());

        var request = new GetProfileRequest { UserId = "507f1f77bcf86cd799439011" };
        var result = await _service.GetProfile(request, CreateContext());

        result.Profile.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetProfile_UserNotFound_ReturnsFailure()
    {
        SetupFindReturnsUser(null);

        var request = new GetProfileRequest { UserId = "507f1f77bcf86cd799439099" };
        var result = await _service.GetProfile(request, CreateContext());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("User not found");
    }

    [Fact]
    public async Task GetProfile_UserExists_ReturnsCorrectRole()
    {
        SetupFindReturnsUser(CreateTestUser());

        var request = new GetProfileRequest { UserId = "507f1f77bcf86cd799439011" };
        var result = await _service.GetProfile(request, CreateContext());

        result.Profile.Role.Should().Be("tourist");
    }

    // ── GetUserById ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserById_UserExists_ReturnsSuccess()
    {
        SetupFindReturnsUser(CreateTestUser());

        var request = new GetUserByIdRequest { UserId = "507f1f77bcf86cd799439011" };
        var result = await _service.GetUserById(request, CreateContext());

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserById_UserExists_ReturnsCorrectUsername()
    {
        SetupFindReturnsUser(CreateTestUser());

        var request = new GetUserByIdRequest { UserId = "507f1f77bcf86cd799439011" };
        var result = await _service.GetUserById(request, CreateContext());

        result.User.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetUserById_UserNotFound_ReturnsFailure()
    {
        SetupFindReturnsUser(null);

        var request = new GetUserByIdRequest { UserId = "507f1f77bcf86cd799439099" };
        var result = await _service.GetUserById(request, CreateContext());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("User not found");
    }

    // ── UpdateProfile ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProfile_UserExists_ReturnsSuccess()
    {
        SetupFindReturnsUser(CreateTestUser());
        SetupUpdateSucceeds();

        var request = new UpdateProfileRequest
        {
            UserId = "507f1f77bcf86cd799439011",
            FirstName = "Updated",
            LastName = "Name",
            Bio = "Updated bio",
            Motto = "Updated motto",
            ProfilePicture = ""
        };

        var result = await _service.UpdateProfile(request, CreateContext());

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProfile_UserNotFound_ReturnsFailure()
    {
        SetupFindReturnsUser(null);

        var request = new UpdateProfileRequest
        {
            UserId = "507f1f77bcf86cd799439099",
            FirstName = "Updated",
            LastName = "Name",
            Bio = "Updated bio",
            Motto = "Updated motto",
            ProfilePicture = ""
        };

        var result = await _service.UpdateProfile(request, CreateContext());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("User not found");
    }

    [Fact]
    public async Task UpdateProfile_UserExists_ReturnsUpdatedFirstName()
    {
        SetupFindReturnsUser(CreateTestUser());
        SetupUpdateSucceeds();

        var request = new UpdateProfileRequest
        {
            UserId = "507f1f77bcf86cd799439011",
            FirstName = "NewName",
            LastName = "User",
            Bio = "bio",
            Motto = "motto",
            ProfilePicture = ""
        };

        var result = await _service.UpdateProfile(request, CreateContext());

        result.Profile.FirstName.Should().Be("NewName");
    }
}