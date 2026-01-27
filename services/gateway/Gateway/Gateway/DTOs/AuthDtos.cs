namespace Gateway.DTOs;

// Registration
public class RegisterRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "guide" or "tourist"
}

public class RegisterResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? UserId { get; set; }
}

// Login
public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string? UserId { get; set; }
    public string? Role { get; set; }
}

// Profile
public class UserProfileDto
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string ProfilePicture { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Motto { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
}

public class UpdateProfileRequestDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string ProfilePicture { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Motto { get; set; } = string.Empty;
}

public class ProfileResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserProfileDto? Profile { get; set; }
}
