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

public class AuthGrpcServiceAdminTests
{
    private readonly Mock<IMongoCollection<User>> _usersCollectionMock;
    private readonly Mock<ILogger<AuthGrpcService>> _loggerMock;
    private readonly AuthGrpcService _service;

    private static readonly User AdminUser = new()
    {
        Id = "507f1f77bcf86cd799439000",
        Username = "admin",
        Email = "admin@example.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
        Role = "admin",
        IsBlocked = false,
        Profile = new UserProfileData()
    };

    private static readonly User RegularUser = new()
    {
        Id = "507f1f77bcf86cd799439011",
        Username = "testuser",
        Email = "test@example.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
        Role = "tourist",
        IsBlocked = false,
        Profile = new UserProfileData()
    };

    public AuthGrpcServiceAdminTests()
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

        var tokenService = new TokenService(jwtSettings);
        _service = new AuthGrpcService(
            _usersCollectionMock.Object,
            tokenService,
            _loggerMock.Object);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

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

    private void SetupFindReturnsSequence(params User?[] users)
    {
        var callCount = 0;
        _usersCollectionMock
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<FindOptions<User, User>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var user = users[callCount < users.Length ? callCount++ : users.Length - 1];
                var cursorMock = new Mock<IAsyncCursor<User>>();
                cursorMock
                    .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(user != null)
                    .ReturnsAsync(false);
                cursorMock
                    .Setup(c => c.Current)
                    .Returns(user != null ? new[] { user } : Array.Empty<User>());
                return cursorMock.Object;
            });
    }

    private void SetupReplaceSucceeds()
    {
        _usersCollectionMock
            .Setup(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<User>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));
    }

    // ── GetAllUsers ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllUsers_NonAdminUser_ReturnsUnauthorized()
    {
        SetupFindReturnsUser(null); // admin not found

        var request = new GetAllUsersRequest { AdminUserId = "nonexistent" };
        var result = await _service.GetAllUsers(request, CreateContext());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Unauthorized");
    }

    [Fact]
    public async Task GetAllUsers_ValidAdmin_ReturnsSuccess()
    {
        // First call returns admin, second call returns list of users
        var allUsersCursor = new Mock<IAsyncCursor<User>>();
        allUsersCursor
            .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        allUsersCursor
            .Setup(c => c.Current)
            .Returns(new[] { AdminUser, RegularUser });

        var adminCursor = new Mock<IAsyncCursor<User>>();
        adminCursor
            .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        adminCursor
            .Setup(c => c.Current)
            .Returns(new[] { AdminUser });

        var callCount = 0;
        _usersCollectionMock
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<FindOptions<User, User>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => callCount++ == 0 ? adminCursor.Object : allUsersCursor.Object);

        var request = new GetAllUsersRequest { AdminUserId = AdminUser.Id };
        var result = await _service.GetAllUsers(request, CreateContext());

        result.Success.Should().BeTrue();
        result.Users.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAllUsers_ValidAdmin_ReturnsAllUsers()
    {
        var allUsersCursor = new Mock<IAsyncCursor<User>>();
        allUsersCursor
            .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        allUsersCursor
            .Setup(c => c.Current)
            .Returns(new[] { AdminUser, RegularUser });

        var adminCursor = new Mock<IAsyncCursor<User>>();
        adminCursor
            .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        adminCursor
            .Setup(c => c.Current)
            .Returns(new[] { AdminUser });

        var callCount = 0;
        _usersCollectionMock
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<FindOptions<User, User>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => callCount++ == 0 ? adminCursor.Object : allUsersCursor.Object);

        var request = new GetAllUsersRequest { AdminUserId = AdminUser.Id };
        var result = await _service.GetAllUsers(request, CreateContext());

        result.Users.Count.Should().Be(2);
    }

    // ── BlockUser ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task BlockUser_NonAdminRequester_ReturnsUnauthorized()
    {
        SetupFindReturnsUser(null); // admin lookup returns nothing

        var request = new BlockUserRequest
        {
            AdminUserId = "notanadmin",
            TargetUserId = RegularUser.Id,
            Block = true
        };

        var result = await _service.BlockUser(request, CreateContext());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Unauthorized");
    }

    [Fact]
    public async Task BlockUser_TargetUserNotFound_ReturnsFailure()
    {
        SetupFindReturnsSequence(AdminUser, null);

        var request = new BlockUserRequest
        {
            AdminUserId = AdminUser.Id,
            TargetUserId = "nonexistent",
            Block = true
        };

        var result = await _service.BlockUser(request, CreateContext());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("User not found");
    }

    [Fact]
    public async Task BlockUser_TargetIsAdmin_ReturnsFailure()
    {
        var anotherAdmin = new User
        {
            Id = "507f1f77bcf86cd799439099",
            Username = "admin2",
            Email = "admin2@example.com",
            Role = "admin",
            IsBlocked = false,
            Profile = new UserProfileData()
        };

        SetupFindReturnsSequence(AdminUser, anotherAdmin);

        var request = new BlockUserRequest
        {
            AdminUserId = AdminUser.Id,
            TargetUserId = anotherAdmin.Id,
            Block = true
        };

        var result = await _service.BlockUser(request, CreateContext());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot block admin");
    }

    [Fact]
    public async Task BlockUser_ValidRequest_ReturnsSuccess()
    {
        SetupFindReturnsSequence(AdminUser, RegularUser);
        SetupReplaceSucceeds();

        var request = new BlockUserRequest
        {
            AdminUserId = AdminUser.Id,
            TargetUserId = RegularUser.Id,
            Block = true
        };

        var result = await _service.BlockUser(request, CreateContext());

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("blocked");
    }

    [Fact]
    public async Task BlockUser_UnblockRequest_ReturnsUnblockedMessage()
    {
        var blockedUser = new User
        {
            Id = RegularUser.Id,
            Username = RegularUser.Username,
            Email = RegularUser.Email,
            Role = "tourist",
            IsBlocked = true,
            Profile = new UserProfileData()
        };

        SetupFindReturnsSequence(AdminUser, blockedUser);
        SetupReplaceSucceeds();

        var request = new BlockUserRequest
        {
            AdminUserId = AdminUser.Id,
            TargetUserId = blockedUser.Id,
            Block = false
        };

        var result = await _service.BlockUser(request, CreateContext());

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("unblocked");
    }
}
