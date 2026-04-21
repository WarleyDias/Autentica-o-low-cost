# Development Guide

## Initial Setup

### 1. Clone the Repository
```bash
git clone https://github.com/seu-usuario/auth-system.git
cd auth-system
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Run Tests
```bash
dotnet test
```

### 4. Run the Application
```bash
# Development
dotnet run --environment Development

# Production (simulated)
dotnet run --environment Production
```

## Architecture

### Layers

1. **Controllers** - HTTP endpoints
2. **Services** - Business logic
3. **Models** - Entities and DTOs
4. **Configuration** - Application configuration

## Authentication Flow

```
Client
  ↓
POST /api/auth/register or /api/auth/login
  ↓
AuthController
  ↓
AuthService (validations)
  ↓
PasswordHashService (password hashing with BCrypt)
  ↓
JwtTokenService (token generation)
  ↓
Response with Token (if successful)
```

## Code Standards

- **Namespaces**: PascalCase (ex: `AuthSystem.Services`)
- **Classes**: PascalCase (ex: `AuthService`)
- **Interfaces**: "I" prefix + PascalCase (ex: `IAuthService`)
- **Methods**: PascalCase (ex: `LoginAsync`)
- **Private Variables**: Underscore + camelCase (ex: `_authService`)

## Future Improvements

### Short-term
- Add unit tests
- Implement logging with Serilog
- Add validation with FluentValidation
- Create Database Service with Entity Framework

### Medium-term
- Implement Refresh Tokens
- Add social authentication (Google, Facebook)
- Implement 2FA
- Add rate limiting

### Long-term
- Containerize with Docker
- Integrate with Azure or AWS
- Implement distributed cache
- Add security analysis

## Testing

### Run All Tests
```bash
dotnet test
```

### Run Tests with Coverage
```bash
dotnet test /p:CollectCoverage=true
```

### Load Testing
```bash
# Use tools like Apache JMeter or k6
```

## Security

### Security Checklist

- JWT Secret configured with strong key
- HTTPS enabled in production
- CORS properly restricted
- Input validation on all endpoints
- Rate limiting implemented
- Activity logging enabled
- Passwords hashed with BCrypt
- Short token expiration time

## Deployment

### Publish for Production
```bash
dotnet publish -c Release
```

### Configure Environment Variables
```bash
export Jwt__SecretKey="your-super-secure-secret-key"
export Jwt__ExpirationMinutes="60"
```

### Docker
```bash
docker build -t auth-system:latest .
docker run -p 80:80 -p 443:443 auth-system:latest
```

## Troubleshooting

### Error: "Secret key not found"
- Check `appsettings.json`
- Ensure `Jwt:SecretKey` is configured

### Error: "Connection refused"
- Verify application is running
- Check the port (default: 5000/5001)

### Error: "Invalid token"
- Verify token hasn't expired
- Verify JWT key is the same on server

## API Documentation

Interactive documentation is available at:
- **Swagger UI**: `http://localhost:5000/swagger`
- **Swagger JSON**: `http://localhost:5000/swagger/v1/swagger.json`

## Contact

For questions or suggestions, open an issue in the repository.
