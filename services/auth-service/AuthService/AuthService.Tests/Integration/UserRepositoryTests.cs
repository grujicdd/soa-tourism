using AuthService.Models;
using AuthService.Services;
using FluentAssertions;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace AuthService.Tests.Integration;

public class UserRepositoryTests : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer;
    private IMongoCollection<User> _usersCollection = null!;
    private AuthGrpcService _service = null!;

    public UserRepositoryTests()
    {
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:5.0")
            .Build();
    }

    // IAsyncLifetime — runs before and after each test class
    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();

        var client = new MongoClient(_mongoContainer.GetConnectionString());
        var database = client.GetDatabase("auth-db-test");
        _usersCollection = database.GetCollection<User>("users");

        var jwtSettings = new JwtSettings
        {
            Secret = "this-is-a-super-secret-key-for-testing-purposes-123!",
            ExpiryHours = 1,
            Issuer = "AuthService",
            Audience = "TourismApp"
        };

        var tokenService = new TokenService(jwtSettings);
        var logger = Microsoft.Extensions.Logging.Abstractions
            .NullLogger<AuthGrpcService>.Instance;

        _service = new AuthGrpcService(_usersCollection, tokenService, logger);
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Grpc.Core.ServerCallContext CreateContext() =>
        Grpc.Core.Testing.TestServerCallContext.Create(
            method: "test",
            host: "localhost",
            deadline: DateTime.UtcNow.AddMinutes(1),
            requestHeaders: new Grpc.Core.Metadata(),
            cancellationToken: CancellationToken.None,
            peer: "localhost",
            authContext: null,
            contextPropagationToken: null,
            writeHeadersFunc: _ => Task.CompletedTask,
            writeOptionsGetter: () => Grpc.Core.WriteOptions.Default,
            writeOptionsSetter: _ => { });

    // ── Insert and Retrieve ───────────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidUser_CanBeFoundInDatabase()
    {
        var request = new AuthService.Protos.RegisterRequest
        {
            Username = "realuser",
            Email = "real@example.com",
            Password = "Password123!",
            Role = "tourist"
        };

        await _service.Register(request, CreateContext());

        var user = await _usersCollection
            .Find(u => u.Username == "realuser")
            .FirstOrDefaultAsync();

        user.Should().NotBeNull();
        user.Email.Should().Be("real@example.com");
        user.Role.Should().Be("tourist");
    }

    [Fact]
    public async Task Register_ValidUser_PasswordIsHashed()
    {
        var request = new AuthService.Protos.RegisterRequest
        {
            Username = "hasheduser",
            Email = "hashed@example.com",
            Password = "Password123!",
            Role = "tourist"
        };

        await _service.Register(request, CreateContext());

        var user = await _usersCollection
            .Find(u => u.Username == "hasheduser")
            .FirstOrDefaultAsync();

        // Password should never be stored as plain text
        user.PasswordHash.Should().NotBe("Password123!");
        BCrypt.Net.BCrypt.Verify("Password123!", user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Register_DuplicateUsername_ReturnsFailure()
    {
        var request = new AuthService.Protos.RegisterRequest
        {
            Username = "duplicateuser",
            Email = "first@example.com",
            Password = "Password123!",
            Role = "tourist"
        };

        await _service.Register(request, CreateContext());

        // Try registering same username again
        var duplicateRequest = new AuthService.Protos.RegisterRequest
        {
            Username = "duplicateuser",
            Email = "second@example.com",
            Password = "Password123!",
            Role = "tourist"
        };

        var result = await _service.Register(duplicateRequest, CreateContext());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Username already exists");
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsFailure()
    {
        var request = new AuthService.Protos.RegisterRequest
        {
            Username = "firstuser",
            Email = "duplicate@example.com",
            Password = "Password123!",
            Role = "tourist"
        };

        await _service.Register(request, CreateContext());

        var duplicateRequest = new AuthService.Protos.RegisterRequest
        {
            Username = "seconduser",
            Email = "duplicate@example.com",
            Password = "Password123!",
            Role = "tourist"
        };

        var result = await _service.Register(duplicateRequest, CreateContext());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Email already exists");
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokenAndUserId()
    {
        // First register the user
        await _service.Register(new AuthService.Protos.RegisterRequest
        {
            Username = "loginuser",
            Email = "login@example.com",
            Password = "Password123!",
            Role = "guide"
        }, CreateContext());

        // Then login
        var result = await _service.Login(new AuthService.Protos.LoginRequest
        {
            Username = "loginuser",
            Password = "Password123!"
        }, CreateContext());

        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.UserId.Should().NotBeNullOrEmpty();
        result.Role.Should().Be("guide");
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsFailure()
    {
        await _service.Register(new AuthService.Protos.RegisterRequest
        {
            Username = "wrongpassuser",
            Email = "wrongpass@example.com",
            Password = "Password123!",
            Role = "tourist"
        }, CreateContext());

        var result = await _service.Login(new AuthService.Protos.LoginRequest
        {
            Username = "wrongpassuser",
            Password = "WrongPassword!"
        }, CreateContext());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid username or password");
    }

    [Fact]
    public async Task GetProfile_AfterRegister_ReturnsCorrectData()
    {
        var registerResult = await _service.Register(
            new AuthService.Protos.RegisterRequest
            {
                Username = "profileuser",
                Email = "profile@example.com",
                Password = "Password123!",
                Role = "tourist"
            }, CreateContext());

        var result = await _service.GetProfile(
            new AuthService.Protos.GetProfileRequest
            {
                UserId = registerResult.UserId
            }, CreateContext());

        result.Success.Should().BeTrue();
        result.Profile.Username.Should().Be("profileuser");
        result.Profile.Email.Should().Be("profile@example.com");
    }

    [Fact]
    public async Task UpdateProfile_ValidData_PersistsToDatabase()
    {
        var registerResult = await _service.Register(
            new AuthService.Protos.RegisterRequest
            {
                Username = "updateuser",
                Email = "update@example.com",
                Password = "Password123!",
                Role = "tourist"
            }, CreateContext());

        await _service.UpdateProfile(
            new AuthService.Protos.UpdateProfileRequest
            {
                UserId = registerResult.UserId,
                FirstName = "Updated",
                LastName = "Name",
                Bio = "My updated bio",
                Motto = "Updated motto",
                ProfilePicture = ""
            }, CreateContext());

        // Read directly from MongoDB to confirm it actually persisted
        var user = await _usersCollection
            .Find(u => u.Id == registerResult.UserId)
            .FirstOrDefaultAsync();

        user.Profile.FirstName.Should().Be("Updated");
        user.Profile.Bio.Should().Be("My updated bio");
    }
}
