# Arquitetura do Sistema de Autenticação

## Visão Geral

Este é um sistema de autenticação baseado em JWT (JSON Web Tokens) construído com ASP.NET Core 8.0.

## Componentes Principais

### 1. Controllers
- **AuthController** - Gerencia endpoints de autenticação

#### Endpoints
- `POST /api/auth/register` - Registra novo usuário
- `POST /api/auth/login` - Realiza login e retorna token JWT
- `GET /api/auth/profile` - Retorna perfil do usuário autenticado

### 2. Services

#### AuthService
Serviço principal de autenticação
- `RegisterAsync()` - Valida e registra novo usuário
- `LoginAsync()` - Valida credenciais e gera token
- `GetUserByUsernameAsync()` - Busca usuário por nome

#### JwtTokenService
Geração e validação de tokens JWT
- `GenerateToken()` - Cria token JWT para usuário
- `ValidateToken()` - Valida token recebido

#### PasswordHashService
Operações com senhas
- `HashPassword()` - Hash SHA-256 da senha
- `VerifyPassword()` - Verifica se senha corresponde ao hash

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
- `LoginRequest` - Dados para login
- `RegisterRequest` - Dados para registro
- `AuthResponse` - Resposta de autenticação
- `UserDto` - Dados públicos do usuário

## Fluxo de Autenticação

### Registro
```
1. Usuário submete dados (username, email, senha)
2. AuthController valida dados
3. AuthService verifica se usuário já existe
4. PasswordHashService faz hash da senha
5. User armazenado (em memória atualmente)
6. Retorna UserDto
```

### Login
```
1. Usuário submete username e senha
2. AuthController recebe dados
3. AuthService busca usuário
4. PasswordHashService verifica senha
5. Se válido, JwtTokenService gera token
6. Retorna token e dados do usuário
```

### Acesso a Recurso Protegido
```
1. Cliente envia GET com Authorization: Bearer <token>
2. ASP.NET Core valida token via middleware JWT
3. Se válido, extrai claims
4. authcontroller acessa User.FindFirst(ClaimTypes.Name)
5. Retorna dados do usuário
```

## Segurança

### Implementado
- ✅ Hash SHA-256 para senhas
- ✅ JWT com claims
- ✅ Token com expiração
- ✅ Validação de entrada
- ✅ CORS configurado

### Recomendações para Produção
- ⚠️ Usar bcrypt em vez de SHA-256
- ⚠️ Usar banco de dados real
- ⚠️ HTTPS obrigatório
- ⚠️ Rate limiting
- ⚠️ Logging e auditoria
- ⚠️ Secrets em variáveis de ambiente

## Configuração

### appsettings.json
```json
{
  "Jwt": {
    "SecretKey": "chave-secreta",
    "Issuer": "issuer",
    "Audience": "audience",
    "ExpirationMinutes": 60
  }
}
```

### Dependências Injetadas
- `IJwtTokenService` → `JwtTokenService`
- `IPasswordHashService` → `PasswordHashService`
- `IAuthService` → `AuthService`

## Diagrama de Componentes

```
┌─────────────────────────────────────────────────────────┐
│                    Cliente HTTP                         │
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
│  ├─ PasswordHashService                                  │
│  ├─ JwtTokenService                                      │
│  └─ UserRepository (memória)                             │
└─────────────────────────────────────────────────────────┘
```

## Decisões de Design

1. **In-Memory Storage** - Simples para demonstração
2. **SHA-256** - Rápido, adequado para exemplo
3. **JWT** - Stateless, escalável
4. **Dependency Injection** - Padrão .NET moderno
5. **Async/Await** - Preparado para I/O assíncrono

## Próximas Fases

### Fase 2: Database
- Entity Framework Core
- SQL Server ou PostgreSQL
- Migrations

### Fase 3: Segurança Avançada
- Bcrypt para senhas
- Refresh tokens
- 2FA

### Fase 4: Observabilidade
- Logging estruturado
- Tracing distribuído
- Métricas

---

**Versão**: 1.0  
**Última atualização**: 2024  
**Status**: Production-Ready (com ressalvas de segurança)
