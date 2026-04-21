# Auth System

Sistema de autenticacao JWT com refresh tokens persistidos em banco, construido com ASP.NET Core 8.

## Stack

- .NET 8 / ASP.NET Core
- Entity Framework Core + SQLite
- BCrypt.Net-Next (hash de senhas, work factor 12)
- JWT Bearer (access token curto) + Refresh Token (longa duracao)

## Fluxo de autenticacao

```
POST /api/auth/register  -> cria usuario no banco
POST /api/auth/login     -> retorna access token (JWT, 15min) + refresh token (7 dias)
POST /api/auth/refresh   -> valida refresh token, rotaciona, retorna novo par
POST /api/auth/revoke    -> invalida refresh token especifico
POST /api/auth/logout    -> invalida refresh token (requer JWT valido)
GET  /api/auth/profile   -> dados do usuario autenticado (requer JWT valido)
```

## Seguranca dos refresh tokens

**Armazenamento:** apenas SHA-256 do token e salvo no banco. O valor raw trafega via HTTPS e nunca persiste.

**Rotacao:** a cada `/refresh`, o token atual e revogado e um novo e emitido na mesma `family` (identificador de sessao de login).

**Deteccao de reutilizacao:** se um token ja revogado for apresentado, toda a `family` e revogada imediatamente. O cenario indica token roubado - o atacante usou antes do cliente legitimo, ou o cliente tentou reusar um token ja rotacionado. A resposta e forcar re-autenticacao completa.

## Configuracao

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=auth.db"
  },
  "Jwt": {
    "SecretKey": "chave-minimo-32-caracteres-trocar-em-producao",
    "Issuer": "AuthSystem",
    "Audience": "AuthSystemUsers",
    "ExpirationMinutes": 15
  },
  "RefreshToken": {
    "ExpirationDays": 7
  }
}
```

Em producao, `SecretKey` deve vir de variavel de ambiente ou secrets manager, nunca do repositorio.

## Rodar localmente

```bash
dotnet restore auth-system.csproj
dotnet run --project auth-system.csproj
```

O banco SQLite (`auth.db`) e criado automaticamente via `EnsureCreated()` na inicializacao. Swagger disponivel em `/swagger` no ambiente Development.

## Endpoints

### POST /api/auth/register

```json
{
  "username": "joao",
  "email": "joao@exemplo.com",
  "password": "Senha123!",
  "confirmPassword": "Senha123!"
}
```

### POST /api/auth/login

```json
{ "username": "joao", "password": "Senha123!" }
```

Resposta:
```json
{
  "success": true,
  "accessToken": "eyJ...",
  "refreshToken": "base64url...",
  "expiresIn": 900
}
```

### POST /api/auth/refresh

```json
{ "refreshToken": "base64url..." }
```

Retorna novo par `accessToken` + `refreshToken`. O token anterior e invalidado.

### POST /api/auth/revoke

```json
{ "refreshToken": "base64url..." }
```

Retorna `204 No Content`.

### POST /api/auth/logout

Requer `Authorization: Bearer <access_token>`. Corpo opcional com `refreshToken` para revogar a sessao atual.

### GET /api/auth/profile

Requer `Authorization: Bearer <access_token>`.

## Auditoria e logs de seguranca

Eventos de seguranca sao persistidos na tabela `AuditLogs` e emitidos como logs estruturados via `ILogger`.

### Eventos registrados

| EventType | Nivel | DB | Descricao |
|-----------|-------|----|-----------|
| `LOGIN_SUCCESS` | Info | Sim | Login bem-sucedido |
| `LOGIN_FAILED` | Warning | Sim | Credenciais invalidas (detail: `user_not_found` ou `invalid_password`) |
| `TOKEN_REUSE_DETECTED` | Critical | Sim | Refresh token ja revogado apresentado - toda a family e derrubada |
| `TOKEN_REVOKED` | Info | Sim | Revogacao explicita via `/revoke` ou `/logout` |
| `JWT_VALIDATION_FAILED` | Warning | Sim | Token JWT com assinatura invalida, expirado ou malformado |

Token refresh bem-sucedido e logado apenas via `ILogger` (sem DB) por ser evento de alto volume operacional.

### Formato do log estruturado

```
[AUDIT] Event={EventType} Success={Success} UserId={UserId} Username={Username} IP={IpAddress} Details={Details}
```

### Isolamento do contexto de auditoria

`AuditLogService` usa `IDbContextFactory<AppDbContext>` para criar um DbContext proprio em cada escrita. Isso garante que uma falha no contexto principal do request (ex: violacao de constraint) nao impeça o registro do evento de auditoria. O log estruturado e sempre escrito primeiro; a persistencia em DB e best-effort com fallback para `LogError`.

### Consultas uteis

```sql
-- Tentativas de login falhas nas ultimas 24h por IP
SELECT IpAddress, COUNT(*) as attempts
FROM AuditLogs
WHERE EventType = 'LOGIN_FAILED'
  AND CreatedAt > datetime('now', '-1 day')
GROUP BY IpAddress
ORDER BY attempts DESC;

-- Incidentes de reutilizacao de token
SELECT * FROM AuditLogs
WHERE EventType = 'TOKEN_REUSE_DETECTED'
ORDER BY CreatedAt DESC;

-- Historico de um usuario especifico
SELECT EventType, IpAddress, Success, Details, CreatedAt
FROM AuditLogs
WHERE UserId = 5
ORDER BY CreatedAt DESC;
```

Em producao, considere adicionar retencao automatica (ex: deletar registros com mais de 90 dias) para evitar crescimento ilimitado da tabela.

## Estrutura

```
Controllers/
  AuthController.cs           endpoints de autenticacao
Data/
  AppDbContext.cs             EF Core DbContext + mapeamento das entidades
Models/
  User.cs                     entidade de usuario
  RefreshToken.cs             entidade de refresh token (sem campo raw)
  AuditLog.cs                 entidade de auditoria + SecurityEvent constants
  TokenResponse.cs            DTO de resposta com par de tokens
Services/
  AuthService.cs              logica de negocio: register, login, refresh, revoke
  RefreshTokenService.cs      criacao, rotacao e revogacao de tokens
  AuditLogService.cs          persistencia e emissao de eventos de seguranca
  JwtTokenService.cs          geracao e validacao de JWT
  PasswordHashService.cs      wrapper BCrypt
```

## Dependencias

- `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.0
- `Microsoft.EntityFrameworkCore.Sqlite` 8.0.0
- `BCrypt.Net-Next` 4.0.3
- `Swashbuckle.AspNetCore` 6.5.0
