using AuthService.Models;
using AuthService.Protos;
using Grpc.Core;
using MongoDB.Driver;

namespace AuthService.Services;

public class AuthGrpcService : AuthService.Protos.AuthService.AuthServiceBase
{
    private readonly IMongoCollection<User> _usersCollection;
    private readonly TokenService _tokenService;
    private readonly ILogger<AuthGrpcService> _logger;

    public AuthGrpcService(
        IMongoCollection<User> usersCollection,
        TokenService tokenService,
        ILogger<AuthGrpcService> logger)
    {
        _usersCollection = usersCollection;
        _tokenService = tokenService;
        _logger = logger;
    }

    public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        try
        {
            // Validate role
            if (request.Role != "guide" && request.Role != "tourist")
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Invalid role. Must be 'guide' or 'tourist'."
                };
            }

            // Check if username exists
            var existingUser = await _usersCollection
                .Find(u => u.Username == request.Username)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Username already exists."
                };
            }

            // Check if email exists
            existingUser = await _usersCollection
                .Find(u => u.Email == request.Email)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Email already exists."
                };
            }

            // Create new user
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                IsBlocked = false,
                CreatedAt = DateTime.UtcNow
            };

            await _usersCollection.InsertOneAsync(user);

            _logger.LogInformation($"User {user.Username} registered successfully with role {user.Role}");

            return new RegisterResponse
            {
                Success = true,
                Message = "Registration successful.",
                UserId = user.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during registration: {ex.Message}");
            return new RegisterResponse
            {
                Success = false,
                Message = "An error occurred during registration."
            };
        }
    }

    public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _usersCollection
                .Find(u => u.Username == request.Username)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid username or password."
                };
            }

            if (user.IsBlocked)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Your account has been blocked."
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid username or password."
                };
            }

            var token = _tokenService.GenerateToken(user);

            _logger.LogInformation($"User {user.Username} logged in successfully");

            return new LoginResponse
            {
                Success = true,
                Message = "Login successful.",
                Token = token,
                UserId = user.Id,
                Role = user.Role
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during login: {ex.Message}");
            return new LoginResponse
            {
                Success = false,
                Message = "An error occurred during login."
            };
        }
    }

    public override async Task<ProfileResponse> GetProfile(GetProfileRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _usersCollection
                .Find(u => u.Id == request.UserId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return new ProfileResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            return new ProfileResponse
            {
                Success = true,
                Message = "Profile retrieved successfully.",
                Profile = MapToUserProfile(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving profile: {ex.Message}");
            return new ProfileResponse
            {
                Success = false,
                Message = "An error occurred while retrieving profile."
            };
        }
    }

    public override async Task<ProfileResponse> UpdateProfile(UpdateProfileRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _usersCollection
                .Find(u => u.Id == request.UserId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return new ProfileResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            user.Profile.FirstName = request.FirstName;
            user.Profile.LastName = request.LastName;
            user.Profile.ProfilePicture = request.ProfilePicture;
            user.Profile.Bio = request.Bio;
            user.Profile.Motto = request.Motto;

            await _usersCollection.ReplaceOneAsync(u => u.Id == request.UserId, user);

            _logger.LogInformation($"Profile updated for user {user.Username}");

            return new ProfileResponse
            {
                Success = true,
                Message = "Profile updated successfully.",
                Profile = MapToUserProfile(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating profile: {ex.Message}");
            return new ProfileResponse
            {
                Success = false,
                Message = "An error occurred while updating profile."
            };
        }
    }

    public override async Task<UsersResponse> GetAllUsers(GetAllUsersRequest request, ServerCallContext context)
    {
        try
        {
            // Verify admin role
            var admin = await _usersCollection
                .Find(u => u.Id == request.AdminUserId && u.Role == "admin")
                .FirstOrDefaultAsync();

            if (admin == null)
            {
                return new UsersResponse
                {
                    Success = false,
                    Message = "Unauthorized. Admin access required."
                };
            }

            var users = await _usersCollection
                .Find(_ => true)
                .ToListAsync();

            var userProfiles = users.Select(MapToUserProfile).ToList();

            return new UsersResponse
            {
                Success = true,
                Message = "Users retrieved successfully.",
                Users = { userProfiles }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving users: {ex.Message}");
            return new UsersResponse
            {
                Success = false,
                Message = "An error occurred while retrieving users."
            };
        }
    }

    public override async Task<BlockUserResponse> BlockUser(BlockUserRequest request, ServerCallContext context)
    {
        try
        {
            // Verify admin role
            var admin = await _usersCollection
                .Find(u => u.Id == request.AdminUserId && u.Role == "admin")
                .FirstOrDefaultAsync();

            if (admin == null)
            {
                return new BlockUserResponse
                {
                    Success = false,
                    Message = "Unauthorized. Admin access required."
                };
            }

            var user = await _usersCollection
                .Find(u => u.Id == request.TargetUserId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return new BlockUserResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            if (user.Role == "admin")
            {
                return new BlockUserResponse
                {
                    Success = false,
                    Message = "Cannot block admin users."
                };
            }

            user.IsBlocked = request.Block;
            await _usersCollection.ReplaceOneAsync(u => u.Id == request.TargetUserId, user);

            var action = request.Block ? "blocked" : "unblocked";
            _logger.LogInformation($"User {user.Username} has been {action} by admin {admin.Username}");

            return new BlockUserResponse
            {
                Success = true,
                Message = $"User {action} successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error blocking user: {ex.Message}");
            return new BlockUserResponse
            {
                Success = false,
                Message = "An error occurred while blocking user."
            };
        }
    }

    public override Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
    {
        try
        {
            var userInfo = _tokenService.GetUserInfoFromToken(request.Token);

            if (userInfo == null)
            {
                return Task.FromResult(new ValidateTokenResponse
                {
                    Valid = false,
                    Message = "Invalid or expired token."
                });
            }

            return Task.FromResult(new ValidateTokenResponse
            {
                Valid = true,
                UserId = userInfo.Value.userId,
                Username = userInfo.Value.username,
                Role = userInfo.Value.role,
                Message = "Token is valid."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error validating token: {ex.Message}");
            return Task.FromResult(new ValidateTokenResponse
            {
                Valid = false,
                Message = "An error occurred while validating token."
            });
        }
    }

    public override async Task<UserResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _usersCollection
                .Find(u => u.Id == request.UserId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return new UserResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            return new UserResponse
            {
                Success = true,
                Message = "User retrieved successfully.",
                User = MapToUserProfile(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving user: {ex.Message}");
            return new UserResponse
            {
                Success = false,
                Message = "An error occurred while retrieving user."
            };
        }
    }

    private static UserProfile MapToUserProfile(User user)
    {
        return new UserProfile
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            FirstName = user.Profile.FirstName,
            LastName = user.Profile.LastName,
            ProfilePicture = user.Profile.ProfilePicture,
            Bio = user.Profile.Bio,
            Motto = user.Profile.Motto,
            IsBlocked = user.IsBlocked
        };
    }
}
