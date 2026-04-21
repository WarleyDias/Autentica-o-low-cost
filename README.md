# Sistema de Autenticação Simples com .NET C#

Um sistema de autenticação seguro e simples construído com ASP.NET Core, utilizando JWT (JSON Web Tokens) para autenticação de API.

## 📋 Funcionalidades

- ✅ Registro de usuários
- ✅ Login com geração de tokens JWT
- ✅ Hash seguro de senhas (SHA-256)
- ✅ Autenticação baseada em Bearer Token
- ✅ Validação de tokens JWT
- ✅ Endpoint protegido para perfil do usuário
- ✅ CORS configurado
- ✅ Swagger/OpenAPI documentação

## 🏗️ Estrutura do Projeto

```
auth-system/
├── Controllers/
│   └── AuthController.cs           # Endpoints de autenticação
├── Models/
│   ├── User.cs                     # Modelo de usuário
│   ├── LoginRequest.cs             # Requisição de login
│   ├── RegisterRequest.cs          # Requisição de registro
│   └── AuthResponse.cs             # Resposta de autenticação
├── Services/
│   ├── AuthService.cs              # Serviço principal de autenticação
│   ├── JwtTokenService.cs          # Geração e validação de tokens JWT
│   └── PasswordHashService.cs      # Hash e verificação de senhas
├── Program.cs                      # Configuração da aplicação
├── appsettings.json                # Configurações de produção
├── appsettings.Development.json    # Configurações de desenvolvimento
└── auth-system.csproj              # Arquivo de projeto
```

## 🚀 Como Usar

### 1. Pré-requisitos

- .NET 8.0 SDK ou superior
- Visual Studio / Visual Studio Code
- Postman ou outra ferramenta para testar API

### 2. Executar o Projeto

```bash
# Restaurar dependências
dotnet restore

# Executar a aplicação
dotnet run

# Em desenvolvimento
dotnet run --environment Development
```

A aplicação estará disponível em `https://localhost:7091` (ou a porta configurada)

### 3. Endpoints da API

#### 📝 Registro de Usuário
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "joao",
  "email": "joao@example.com",
  "password": "senha123",
  "confirmPassword": "senha123"
}
```

**Resposta (Sucesso):**
```json
{
  "success": true,
  "message": "Usuário registrado com sucesso",
  "token": null,
  "user": {
    "id": 1,
    "username": "joao",
    "email": "joao@example.com"
  }
}
```

#### 🔐 Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "joao",
  "password": "senha123"
}
```

**Resposta (Sucesso):**
```json
{
  "success": true,
  "message": "Login realizado com sucesso",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "joao",
    "email": "joao@example.com"
  }
}
```

#### 👤 Perfil do Usuário (Requer Autenticação)
```http
GET /api/auth/profile
Authorization: Bearer <seu_token_jwt>
```

**Resposta:**
```json
{
  "id": 1,
  "username": "joao",
  "email": "joao@example.com"
}
```

## 🔑 Configuração JWT

As configurações JWT estão em `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "sua-chave-secreta-muito-longa",
    "Issuer": "AuthSystem",
    "Audience": "AuthSystemUsers",
    "ExpirationMinutes": 60
  }
}
```

⚠️ **IMPORTANTE**: Em produção, altere a `SecretKey` e use uma chave mais segura.

## 🔒 Segurança

- Senhas são hashadas com SHA-256
- Tokens JWT com expiração configurável
- Validação de claims no token
- CORS configurado
- Validações de entrada

## 📦 Dependências

- `System.IdentityModel.Tokens.Jwt`: Para trabalhar com JWT
- `Microsoft.IdentityModel.Tokens`: Para validação de tokens
- `Microsoft.AspNetCore`: Framework web

## 📝 Notas

- Atualmente, os usuários são armazenados em memória
- Para produção, integre um banco de dados real (SQL Server, PostgreSQL, etc.)
- Use secrets seguros para a chave JWT em produção

## 🤝 Próximos Passos

Para melhorar este sistema:

1. Integrar com um banco de dados real (Entity Framework Core)
2. Implementar refresh tokens
3. Adicionar autenticação de dois fatores (2FA)
4. Implementar rate limiting
5. Adicionar logs e auditoria
6. Usar bcrypt para hash de senhas (mais seguro)

## 📄 Licença

MIT
