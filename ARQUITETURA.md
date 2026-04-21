# Authentication System Architecture

## Overview

This is a JWT (JSON Web Tokens) based authentication system built with ASP.NET Core 8.0.

## Main Components

### 1. Controllers
- **AuthController** - Manages authentication endpoints

#### Endpoints
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and return JWT token
- `GET /api/auth/profile` - Return authenticated user profile

### 2. Services

#### AuthService
Main authentication service
- `RegisterAsync()` - Validates and registers new user
- `LoginAsync()` - Validates credentials and generates token
- `GetUserByUsernameAsync()` - Search user by username

#### JwtTokenService
JWT token generation and validation
- `GenerateToken()` - Creates JWT token for user
- `ValidateToken()` - Validates received token

#### PasswordHashService
Password operations with BCrypt
- `HashPassword()` - Hash password with BCrypt
- `VerifyPassword()` - Verify password matches hash

### 3. Models

#### User
```csharp
public class User
{
    public int Id
    public string Username
    public string Email
    public string PasswordHash
    public DateTime CreatedAt
    public bool IsActive
}
```

#### DTOs
- `LoginRequest` - Login data
- `RegisterRequest` - Registration data
- `AuthResponse` - Authentication response
- `UserDto` - Public user data

## Authentication Flow

### Registration
```
1. User submits data (username, email, password)
2. AuthController validates data
3. AuthService checks if user already exists
4. PasswordHashService hashes password with BCrypt
5. User stored (in memory currently)
6. Returns UserDto
```

### Login
```
1. User submits username and password
2. AuthController receives data
3. AuthService searches for user
4. PasswordHashService verifies password
5. If valid, JwtTokenService generates token
6. Returns token and user data
```

### Accessing Protected Resource
```
1. Client sends GET with Authorization: Bearer <token>
2. ASP.NET Core validates token via JWT middleware
3. If valid, extracts claims
4. AuthController accesses User.FindFirst(ClaimTypes.Name)
5. Returns user data
```

## Security

### Implemented
- Password hashing with BCrypt (industry standard)
- JWT with claims
- Token expiration
- Input validation
- CORS configured
- HTTPS enforced
- HSTS headers enabled

### Production Recommendations
- Use strong, unique JWT secret key
- Use real database instead of in-memory
- HTTPS is mandatory (not optional)
- Implement rate limiting
- Comprehensive logging and audit trails
- Implement secrets management
- Rotate keys regularly

## Configuration

### appsettings.json
```json
{
  "Jwt": {
    "SecretKey": "secret-key",
    "Issuer": "issuer",
    "Audience": "audience",
    "ExpirationMinutes": 60
  }
}
```

### Dependency Injection
- `IJwtTokenService` → `JwtTokenService`
- `IPasswordHashService` → `PasswordHashService`
- `IAuthService` → `AuthService`
- `IHealthCheckService` → `HealthCheckService`

## Component Diagram

```
┌─────────────────────────────────────────────────────────┐
│                    HTTP Client                          │
└────────────────────┬────────────────────────────────────┘
                     │
┌─────────────────────▼────────────────────────────────────┐
│              AuthController                              │
│  (POST register, login | GET profile)                   │
└────────────────────┬────────────────────────────────────┘
                     │
┌─────────────────────▼────────────────────────────────────┐
│              AuthService                                 │
│  (RegisterAsync, LoginAsync, GetUserByUsernameAsync)   │
├─────────────────────────────────────────────────────────┤
│  ├─ PasswordHashService (BCrypt)                         │
│  ├─ JwtTokenService                                      │
│  └─ User Repository (in-memory)                          │
└─────────────────────────────────────────────────────────┘
```

## Design Decisions

1. **In-Memory Storage** - Simple for demonstration
2. **BCrypt Hashing** - Industry standard algorithm
3. **JWT** - Stateless and scalable
4. **Dependency Injection** - Modern .NET pattern
5. **Async/Await** - Prepared for async I/O

## Next Phases

### Phase 2: Database
- Entity Framework Core
- SQL Server or PostgreSQL
- Database migrations

### Phase 3: Advanced Security
- Refresh tokens
- Two-factor authentication (2FA)
- Role-based access control (RBAC)

### Phase 4: Observability
- Structured logging
- Distributed tracing
- Metrics collection

---

**Version**: 1.0  
**Last Updated**: 2024  
**Status**: Production-Ready with security considerations

