# Auth System

Sistema de autenticacao JWT com refresh tokens, verificacao de email e reset de senha, construido com ASP.NET Core 8.

## Stack

- .NET 8 / ASP.NET Core
- Entity Framework Core + SQLite
- BCrypt.Net-Next (hash de senhas, work factor 12)
- JWT Bearer (access token 15min) + Refresh Token (7 dias)

## Endpoints

```
POST /api/auth/register          cria usuario e envia email de verificacao
POST /api/auth/login             retorna access token + refresh token
POST /api/auth/refresh           rotaciona refresh token, retorna novo par
POST /api/auth/revoke            invalida refresh token
POST /api/auth/logout            invalida refresh token (requer JWT)
POST /api/auth/forgot-password   gera token de reset e envia por email
POST /api/auth/reset-password    redefine senha e revoga todas as sessoes
POST /api/auth/verify-email      confirma email com token recebido
GET  /api/auth/profile           dados do usuario (requer JWT)
```

## Verificacao de email

Ao registrar, um token de verificacao (valido por 24h) e gerado, armazenado como SHA-256 e enviado por email. Em desenvolvimento, o token aparece nos logs do servidor:

```
[EMAIL] To=joao@exemplo.com Subject='Verify your email' Username=joao Token=<raw-token> ExpiresIn=24h
```

Para verificar:
```json
POST /api/auth/verify-email
{ "token": "<raw-token-do-log>" }
```

O login pode ser configurado para bloquear usuarios nao verificados:
```json
"Auth": { "RequireEmailVerification": true }
```

Padrao e `false` para nao quebrar o fluxo em desenvolvimento.

## Reset de senha

```
1. POST /api/auth/forgot-password  { "email": "..." }
   -> sempre retorna 200 (nao revela se email existe)
   -> token valido por 1 hora aparece no log em dev

2. POST /api/auth/reset-password
   { "token": "...", "newPassword": "...", "confirmNewPassword": "..." }
   -> valida token, atualiza senha, revoga TODAS as sessoes ativas
```

Ao redefinir a senha, todos os refresh tokens do usuario sao revogados. Qualquer sessao ativa precisa autenticar novamente.

## Seguranca dos tokens de usuario (reset/verificacao)

- Apenas SHA-256 do token e salvo no banco (mesmo padrao dos refresh tokens)
- Um unico token ativo por tipo por usuario (o anterior e invalidado ao criar novo)
- Token marcado como usado apos primeiro uso valido
- Tokens expirados sao automaticamente rejeitados na validacao

## Seguranca dos refresh tokens

- Rotacao a cada uso: token anterior revogado, novo emitido na mesma `family`
- Deteccao de reutilizacao: token revogado apresentado = `family` inteira derrubada (indica roubo)

## Auditoria

Eventos persistidos na tabela `AuditLogs` e emitidos como logs estruturados:

| EventType | Nivel | Descricao |
|-----------|-------|-----------|
| `LOGIN_SUCCESS` | Info | Login bem-sucedido |
| `LOGIN_FAILED` | Warning | Credenciais invalidas (detail: razao interna) |
| `TOKEN_REUSE_DETECTED` | Critical | Sessao comprometida, family revogada |
| `TOKEN_REVOKED` | Info | Revogacao explicita |
| `JWT_VALIDATION_FAILED` | Warning | JWT invalido em endpoint protegido |
| `PASSWORD_RESET_REQUESTED` | Info/Warning | Solicitacao de reset |
| `PASSWORD_RESET_COMPLETED` | Info | Senha redefinida com sucesso |
| `EMAIL_VERIFIED` | Info | Email confirmado |

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
  },
  "Auth": {
    "RequireEmailVerification": false
  }
}
```

Em producao: `SecretKey` via variavel de ambiente, `RequireEmailVerification: true`, trocar `IEmailService` para implementacao SMTP real.

## Rodar localmente

```bash
dotnet restore auth-system.csproj
dotnet run --project auth-system.csproj
```

Banco SQLite criado automaticamente. Swagger em `/swagger` no ambiente Development.

## Estrutura

```
Controllers/
  AuthController.cs            todos os endpoints de auth
Data/
  AppDbContext.cs              EF Core + configuracao das entidades
Models/
  User.cs                      entidade de usuario (inclui IsEmailVerified)
  RefreshToken.cs              token de sessao (hash only)
  UserToken.cs                 token de uso unico (reset/verificacao)
  AuditLog.cs                  entidade de auditoria + SecurityEvent constants
  AccountRequests.cs           DTOs para reset e verificacao
Services/
  AuthService.cs               sessoes: login, refresh, revoke
  AccountService.cs            conta: register, forgot-password, reset, verify-email
  RefreshTokenService.cs       ciclo de vida dos refresh tokens
  UserTokenService.cs          tokens de uso unico (reset/verificacao)
  AuditLogService.cs           persistencia de eventos de seguranca
  EmailService.cs              interface + LogEmailService (dev)
  JwtTokenService.cs           geracao e validacao de JWT
  PasswordHashService.cs       wrapper BCrypt
```

## Fraquezas conhecidas

**Sem rate limiting.** Os endpoints de login e forgot-password nao tem limite de requisicoes. Um atacante pode tentar senhas ilimitadamente ou enviar milhares de emails de reset. A mitigacao e ASP.NET Core Rate Limiting (built-in desde .NET 7) ou um middleware com Redis.

**Validacao de senha apenas por tamanho.** A regra atual e "minimo 8 caracteres". Em producao e esperado pelo menos uma combinacao de maiuscula, minuscula, numero e caractere especial, ou uso de entropia (zxcvbn).

**Sem bloqueio de conta.** Nao ha lockout apos N tentativas falhas de login. Com os dados de auditoria ja existentes, seria simples implementar: contar `LOGIN_FAILED` por username nos ultimos 15 minutos e bloquear temporariamente.

**SQLite nao e adequado para producao.** SQLite serializa escritas — um unico writer por vez. Em qualquer cenario de concorrencia real, use PostgreSQL ou SQL Server. A troca e simples: mudar o provider no `Program.cs` e a connection string.

**`EnsureCreated()` nao permite evolucao do schema.** Se uma coluna for adicionada ao modelo, o banco existente nao e atualizado. Em producao, use `dotnet ef migrations add` e `Database.MigrateAsync()`. `EnsureCreated()` e valido apenas para projetos de demonstracao.

**JWT secret em `appsettings.json`.** O valor atual e um placeholder. Em producao, a secret deve vir de variavel de ambiente (`Jwt__SecretKey`) ou de um secrets manager (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault).

**`IEmailService` sem implementacao real.** `LogEmailService` escreve nos logs do servidor. Para producao, implementar `SmtpEmailService` (MailKit) ou integrar SendGrid/SES via seu SDK.

**Tokens de usuario sem cleanup.** Tokens expirados e usados acumulam na tabela `UserTokens`. Um background job (Hangfire, `IHostedService`) para deletar tokens com mais de X dias evita crescimento ilimitado.

**CORS `AllowAnyOrigin`.** A politica atual aceita requisicoes de qualquer origem. Em producao, especificar as origens permitidas explicitamente.

## Melhorias sugeridas

**Rate limiting por IP e por usuario.** ASP.NET Core 7+ tem rate limiting built-in com `AddRateLimiter`. Para cenarios distribuidos (multiplas instancias), usar Redis como backing store com `AspNetCoreRateLimit` ou similar.

**TOTP 2FA.** Adicionar autenticacao de dois fatores compativel com Google Authenticator usando `Otp.NET`. O fluxo: usuario ativa 2FA (gera secret, mostra QR code), login passa a exigir TOTP alem de senha. Nao requer servico externo.

**Serilog para logging estruturado.** Substituir o `ILogger` padrao por Serilog com sinks para arquivo rotativo e, em producao, Elasticsearch ou Seq. Habilita correlacao de requests por `TraceId` e busca por campos estruturados.

**Migracao para PostgreSQL.** Mudar de SQLite para PostgreSQL resolve o problema de concorrencia e habilita features de producao (connection pooling com Npgsql, row-level locking, particionamento de tabelas para AuditLogs).

**HttpOnly cookies para refresh token.** O refresh token retornado no body JSON fica exposto ao JavaScript da pagina (XSS). O padrao mais seguro e enviar o refresh token em um cookie `HttpOnly; Secure; SameSite=Strict`, tornando-o inacessivel ao JS. Requer ajuste no controller e no cliente.

**Cleanup de tokens expirados.** Implementar `IHostedService` que roda periodicamente (ex: a cada hora) deletando `RefreshToken` e `UserToken` expirados ha mais de 30 dias. Evita crescimento ilimitado e melhora performance de queries.

**OpenTelemetry.** Adicionar tracing distribuido com `OpenTelemetry.Extensions.Hosting` para correlacionar requests entre servicos e rastrear latencia de operacoes criticas (login, consulta ao banco).

## Dependencias

- `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.0
- `Microsoft.EntityFrameworkCore.Sqlite` 8.0.0
- `BCrypt.Net-Next` 4.0.3
- `Swashbuckle.AspNetCore` 6.5.0
