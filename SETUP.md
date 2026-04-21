# Installation and Setup Guide

## Prerequisites

- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download)
- **Git** (optional) - [Download here](https://git-scm.com/)
- **Editor**: Visual Studio, VS Code or other

## Quick Installation

### 1. Clone or Download

```bash
# With Git
git clone https://github.com/seu-usuario/auth-system.git
cd auth-system

# Or download and extract manually
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Run Application

```bash
dotnet run
```

The application will open at `https://localhost:7091` (default port)

## Project Dependencies

Dependencies are already configured in the `.csproj` file:

```xml
<PackageReference Include="Microsoft.AspNetCore.App" Version="8.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.3" />
<PackageReference Include="Microsoft.IdentityModel.Tokens" Version="7.0.3" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
```

### What each package does:

- **Microsoft.AspNetCore.App** - Web framework
- **System.IdentityModel.Tokens.Jwt** - JWT creation and manipulation
- **Microsoft.IdentityModel.Tokens** - Token validation
- **BCrypt.Net-Next** - Secure password hashing

## Configuration

### 1. appsettings.json

The file already has default configuration. For development:

```json
{
  "Jwt": {
    "SecretKey": "dev-secret-key-only-for-development-purposes",
    "Issuer": "AuthSystem",
    "Audience": "AuthSystemUsers",
    "ExpirationMinutes": 60
  }
}
```

### 2. Environment Variables (Production)

Configure on your server:

```bash
# Linux/macOS
export Jwt__SecretKey="your-very-secure-key"
export Jwt__ExpirationMinutes="60"

# Windows PowerShell
$env:Jwt__SecretKey = "your-very-secure-key"
$env:Jwt__ExpirationMinutes = "60"

# Windows CMD
set Jwt__SecretKey=your-very-secure-key
set Jwt__ExpirationMinutes=60
```

## Running the Application

### Development

```bash
# With automatic code reload
dotnet watch run --environment Development
```

### Production (Simulated)

```bash
dotnet run --environment Production
```

### Release Build

```bash
dotnet publish -c Release -o ./publish
cd ./publish
dotnet auth-system.dll
```

## Using Docker

### Build Image

```bash
docker build -t auth-system:latest .
```

### Run Container

```bash
docker run -p 5000:80 \
  -e Jwt__SecretKey="your-key-here" \
  auth-system:latest
```

### Using Docker Compose

```bash
docker-compose up
```

## Verify Installation

### 1. Health Check

```bash
curl http://localhost:5000/api/healthcheck
```

### 2. Swagger/OpenAPI

Open in your browser:
```
http://localhost:5000/swagger
```

### 3. Test Endpoint

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"test","email":"test@example.com","password":"password123","confirmPassword":"password123"}'
```

## Troubleshooting

### Error: "dotnet: command not found"
```bash
# Check .NET installation
dotnet --version

# Install .NET 8.0 if you don't have it
```

### Error: "Port 5000 already in use"
```bash
# Linux/macOS - Find process
lsof -i :5000
kill -9 <PID>

# Windows PowerShell
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

### Error: "Connection refused"
- Ensure the application is running
- Check firewall configuration
- Verify you're using HTTPS (port 5001) or HTTP (port 5000)

### Error: "Invalid Jwt Token"
- Verify SecretKey is the same everywhere
- Verify token hasn't expired
- Ensure token is in correct header: `Authorization: Bearer <token>`

## Created Directory Structure

```
auth-system/
├── bin/                           (Compilation output)
├── obj/                           (Compiled objects)
├── Controllers/
│   ├── AuthController.cs
│   └── HealthCheckController.cs
├── Models/
│   ├── User.cs
│   ├── LoginRequest.cs
│   ├── RegisterRequest.cs
│   ├── AuthResponse.cs
│   └── HealthCheckResponse.cs
├── Services/
│   ├── AuthService.cs
│   ├── JwtTokenService.cs
│   ├── PasswordHashService.cs
│   └── HealthCheckService.cs
├── Program.cs
├── auth-system.csproj
├── appsettings.json
├── appsettings.Development.json
├── Dockerfile
├── docker-compose.yml
├── .gitignore
├── README.md
├── DESENVOLVIMENTO.md
├── ARQUITETURA.md
└── SETUP.md (this file)
```

## Security - Next Steps

1. **Generate new secure SecretKey**
   ```bash
   # Linux/macOS
   openssl rand -base64 32
   
   # Windows PowerShell
   [Convert]::ToBase64String((1..32 | ForEach-Object {[byte](Get-Random -Maximum 256)}))
   ```

2. **Enable HTTPS**
   - Self-signed certificate for development
   - Valid certificate for production (Let's Encrypt)

3. **Implement Database**
   - Remove in-memory storage
   - Use Entity Framework Core
   - SQL Server or PostgreSQL

4. **Add Logging**
   - Serilog for structured logs
   - Track login attempts

## Useful Resources

- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core/)
- [JWT.IO](https://jwt.io/) - Decode tokens
- [Azure App Service](https://azure.microsoft.com) - Deploy
- [Postman](https://www.postman.com/) - Test API

## Frequently Asked Questions

**Q: How to change the port?**  
A: Edit `Properties/launchSettings.json` or use `--urls` flag

**Q: How to connect a database?**  
A: Add Entity Framework Core and create a `DbContext`

**Q: How to add more roles/permissions?**  
A: Extend the `User` model and add claims to JWT

---

**Ready!** Your authentication system should be working. See [README.md](README.md) for more details.

