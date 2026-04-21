using AuthSystem.Models;
using System.Text.RegularExpressions;

namespace AuthSystem.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<User?> GetUserByUsernameAsync(string username);
}

public class AuthService : IAuthService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHashService _passwordHashService;
    private readonly List<User> _users;

    private static readonly Regex EmailValidation = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled);

    public AuthService(IJwtTokenService jwtTokenService, IPasswordHashService passwordHashService)
    {
        _jwtTokenService = jwtTokenService;
        _passwordHashService = passwordHashService;
        _users = new List<User>();
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        await Task.Delay(0);

        if (string.IsNullOrWhiteSpace(request.Username) || 
            string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password))
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Username, email and password are required" 
            };
        }

        if (request.Password != request.ConfirmPassword)
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Passwords do not match" 
            };
        }

        if (request.Password.Length < 8)
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Password must have at least 8 characters" 
            };
        }

        if (!EmailValidation.IsMatch(request.Email))
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Invalid email format" 
            };
        }

        if (request.Username.Length < 3 || request.Username.Length > 50)
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Username must be between 3 and 50 characters" 
            };
        }

        if (_users.Any(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase)))
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Username already exists" 
            };
        }

        if (_users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Email already registered" 
            };
        }

        try
        {
            var user = new User
            {
                Id = _users.Count + 1,
                Username = request.Username.Trim(),
                Email = request.Email.Trim().ToLower(),
                PasswordHash = _passwordHashService.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _users.Add(user);

            return new AuthResponse 
            { 
                Success = true, 
                Message = "User registered successfully",
                User = new UserDto 
                { 
                    Id = user.Id, 
                    Username = user.Username, 
                    Email = user.Email 
                }
            };
        }
        catch
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "An error occurred during registration" 
            };
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        await Task.Delay(0);

        if (string.IsNullOrWhiteSpace(request.Username) || 
            string.IsNullOrWhiteSpace(request.Password))
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Username and password are required" 
            };
        }

        var user = _users.FirstOrDefault(u => 
            u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase));

        if (user == null || !user.IsActive)
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Invalid credentials" 
            };
        }

        if (!_passwordHashService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Invalid credentials" 
            };
        }

        try
        {
            var token = _jwtTokenService.GenerateToken(user);

            return new AuthResponse 
            { 
                Success = true, 
                Message = "Login successful",
                Token = token,
                User = new UserDto 
                { 
                    Id = user.Id, 
                    Username = user.Username, 
                    Email = user.Email 
                }
            };
        }
        catch
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "An error occurred during login" 
            };
        }
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        await Task.Delay(0);
        
        if (string.IsNullOrWhiteSpace(username))
            return null;

        return _users.FirstOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }
}
