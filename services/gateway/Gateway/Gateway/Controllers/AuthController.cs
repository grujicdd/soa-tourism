using Gateway.DTOs;
using Gateway.GrpcClients;
using Microsoft.AspNetCore.Mvc;
using AuthService.Protos;

namespace Gateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthGrpcClient _authClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthGrpcClient authClient, ILogger<AuthController> logger)
    {
        _authClient = authClient;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var grpcRequest = new RegisterRequest
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                Role = request.Role
            };

            var response = await _authClient.Client.RegisterAsync(grpcRequest);

            return Ok(new RegisterResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                UserId = response.UserId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during registration: {ex.Message}");
            return StatusCode(500, new RegisterResponseDto
            {
                Success = false,
                Message = "An error occurred during registration."
            });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var grpcRequest = new LoginRequest
            {
                Username = request.Username,
                Password = request.Password
            };

            var response = await _authClient.Client.LoginAsync(grpcRequest);

            return Ok(new LoginResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Token = response.Token,
                UserId = response.UserId,
                Role = response.Role
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during login: {ex.Message}");
            return StatusCode(500, new LoginResponseDto
            {
                Success = false,
                Message = "An error occurred during login."
            });
        }
    }

    [HttpGet("profile/{userId}")]
    public async Task<ActionResult<ProfileResponseDto>> GetProfile(string userId)
    {
        try
        {
            var grpcRequest = new GetProfileRequest
            {
                UserId = userId
            };

            var response = await _authClient.Client.GetProfileAsync(grpcRequest);

            if (!response.Success)
            {
                return NotFound(new ProfileResponseDto
                {
                    Success = false,
                    Message = response.Message
                });
            }

            return Ok(new ProfileResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Profile = MapToUserProfileDto(response.Profile)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting profile: {ex.Message}");
            return StatusCode(500, new ProfileResponseDto
            {
                Success = false,
                Message = "An error occurred while getting profile."
            });
        }
    }

    [HttpPut("profile/{userId}")]
    public async Task<ActionResult<ProfileResponseDto>> UpdateProfile(string userId, [FromBody] UpdateProfileRequestDto request)
    {
        try
        {
            var grpcRequest = new UpdateProfileRequest
            {
                UserId = userId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                ProfilePicture = request.ProfilePicture,
                Bio = request.Bio,
                Motto = request.Motto
            };

            var response = await _authClient.Client.UpdateProfileAsync(grpcRequest);

            if (!response.Success)
            {
                return NotFound(new ProfileResponseDto
                {
                    Success = false,
                    Message = response.Message
                });
            }

            return Ok(new ProfileResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Profile = MapToUserProfileDto(response.Profile)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating profile: {ex.Message}");
            return StatusCode(500, new ProfileResponseDto
            {
                Success = false,
                Message = "An error occurred while updating profile."
            });
        }
    }

    private static UserProfileDto MapToUserProfileDto(UserProfile profile)
    {
        return new UserProfileDto
        {
            UserId = profile.UserId,
            Username = profile.Username,
            Email = profile.Email,
            Role = profile.Role,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            ProfilePicture = profile.ProfilePicture,
            Bio = profile.Bio,
            Motto = profile.Motto,
            IsBlocked = profile.IsBlocked
        };
    }
}