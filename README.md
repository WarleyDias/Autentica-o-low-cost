# Authentication System with .NET C#

A secure and simple authentication system built with ASP.NET Core, utilizing JWT (JSON Web Tokens) for API authentication.

## Features

- User registration
- Login with JWT token generation
- Secure password hashing using BCrypt
- Bearer Token-based authentication
- JWT token validation
- Protected endpoint for user profile
- CORS configured
- Swagger/OpenAPI documentation
- Health check endpoints
- HTTPS enforced

## Project Structure

```
auth-system/
├── Controllers/
│   ├── AuthController.cs           Authentication endpoints
│   └── HealthCheckController.cs     Health check endpoints
├── Models/
│   ├── User.cs                     User model
│   ├── LoginRequest.cs             Login request DTO
│   ├── RegisterRequest.cs          Registration request DTO
│   ├── AuthResponse.cs             Authentication response
│   └── HealthCheckResponse.cs      Health check response
├── Services/
│   ├── AuthService.cs              Main authentication service
│   ├── JwtTokenService.cs          JWT token generation and validation
│   ├── PasswordHashService.cs      Password hashing with BCrypt
│   └── HealthCheckService.cs       Health check service
├── Program.cs                      Application configuration
├── appsettings.json                Production settings
├── appsettings.Development.json    Development settings
└── auth-system.csproj              Project file
```

## Prerequisites

- .NET 8.0 SDK or higher
- Visual Studio / Visual Studio Code
- Postman or similar tool for testing API

## Running the Project

```bash
# Restore dependencies
dotnet restore

# Run the application
dotnet run

# Run in development mode
dotnet run --environment Development
```

The application will be available at `https://localhost:7091` (or configured port)

## API Endpoints

### Register User

```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePassword123",
  "confirmPassword": "SecurePassword123"
}
```

**Success Response:**
```json
{
  "success": true,
  "message": "User registered successfully",
  "token": null,
  "user": {
    "id": 1,
    "username": "john_doe",
    "email": "john@example.com"
  }
}
```

### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "john_doe",
  "password": "SecurePassword123"
}
```

**Success Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "john_doe",
    "email": "john@example.com"
  }
}
```

### User Profile (Requires Authentication)

```http
GET /api/auth/profile
Authorization: Bearer <your_jwt_token>
```

**Response:**
```json
{
  "id": 1,
  "username": "john_doe",
  "email": "john@example.com"
}
```

### Health Check

```http
GET /api/healthcheck
```

**Response:**
```json
{
  "status": "Ok",
  "timestamp": "2024-01-15T10:30:45.123Z",
  "version": "1.0.0",
  "details": {
    "databaseHealthy": true,
    "authServiceHealthy": true,
    "uptimeMilliseconds": 125430,
    "activeUsers": 0,
    "environment": "Development"
  }
}
```

## JWT Configuration

JWT settings are in `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "your-very-long-secret-key-at-least-32-characters",
    "Issuer": "AuthSystem",
    "Audience": "AuthSystemUsers",
    "ExpirationMinutes": 60
  }
}
```

IMPORTANT: In production, change the `SecretKey` to a strong, unique value.

## Security

- Passwords hashed with BCrypt (industry standard)
- JWT tokens with configurable expiration
- Claims validation in tokens
- CORS properly configured
- Input validation on all endpoints
- HTTPS enforced
- HSTS headers enabled
- Zero clock skew for JWT validation

## Dependencies

- `System.IdentityModel.Tokens.Jwt`: JWT handling
- `Microsoft.IdentityModel.Tokens`: Token validation
- `Microsoft.AspNetCore`: Web framework
- `BCrypt.Net-Next`: Secure password hashing

## Notes

- Users are currently stored in memory
- For production, integrate a real database (SQL Server, PostgreSQL, etc.)
- Use secure secret management for JWT key in production
- Consider implementing database encryption for sensitive data

## Future Improvements

1. Integrate with a real database (Entity Framework Core)
2. Implement refresh tokens
3. Add two-factor authentication (2FA)
4. Implement rate limiting
5. Add comprehensive logging and audit trails
6. Add role-based access control (RBAC)
7. Implement API key authentication option
8. Add request/response logging middleware

## Testing

Use Postman or cURL to test endpoints. See `EXEMPLOS_TESTES.md` for examples.

## License

MIT

