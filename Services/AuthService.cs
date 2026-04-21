using AuthSystem.Models;

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
    private readonly List<User> _users; // Simulando um banco de dados em memória

    public AuthService(IJwtTokenService jwtTokenService, IPasswordHashService passwordHashService)
    {
        _jwtTokenService = jwtTokenService;
        _passwordHashService = passwordHashService;
        _users = new List<User>(); // Em produção, use um banco de dados real
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        await Task.Delay(0); // Simular operação assíncrona

        // Validações
        if (string.IsNullOrWhiteSpace(request.Username) || 
            string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password))
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Usuário, email e senha são obrigatórios" 
            };
        }

        if (request.Password != request.ConfirmPassword)
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "As senhas não coincidem" 
            };
        }

        if (request.Password.Length < 6)
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "A senha deve ter no mínimo 6 caracteres" 
            };
        }

        // Verificar se o usuário já existe
        if (_users.Any(u => u.Username == request.Username))
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Usuário já existe" 
            };
        }

        // Criar novo usuário
        var user = new User
        {
            Id = _users.Count + 1,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHashService.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _users.Add(user);

        return new AuthResponse 
        { 
            Success = true, 
            Message = "Usuário registrado com sucesso",
            User = new UserDto 
            { 
                Id = user.Id, 
                Username = user.Username, 
                Email = user.Email 
            }
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        await Task.Delay(0); // Simular operação assíncrona

        if (string.IsNullOrWhiteSpace(request.Username) || 
            string.IsNullOrWhiteSpace(request.Password))
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Usuário e senha são obrigatórios" 
            };
        }

        var user = _users.FirstOrDefault(u => u.Username == request.Username);

        if (user == null || !user.IsActive)
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Usuário ou senha inválidos" 
            };
        }

        if (!_passwordHashService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Usuário ou senha inválidos" 
            };
        }

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponse 
        { 
            Success = true, 
            Message = "Login realizado com sucesso",
            Token = token,
            User = new UserDto 
            { 
                Id = user.Id, 
                Username = user.Username, 
                Email = user.Email 
            }
        };
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        await Task.Delay(0); // Simular operação assíncrona
        return _users.FirstOrDefault(u => u.Username == username);
    }
}
