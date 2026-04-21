# Guia de Instalação e Setup

## 📋 Pré-requisitos

- **.NET 8.0 SDK** - [Download aqui](https://dotnet.microsoft.com/download)
- **Git** (opcional) - [Download aqui](https://git-scm.com/)
- **Editor**: Visual Studio, VS Code ou outro

## 🚀 Instalação Rápida

### 1. Clonar ou Fazer Download

```bash
# Com Git
git clone https://github.com/seu-usuario/auth-system.git
cd auth-system

# Ou fazer download e extrair manualmente
```

### 2. Restaurar Dependências

```bash
dotnet restore
```

### 3. Executar Testes

```bash
dotnet run
```

A aplicação abrirá em `https://localhost:7091` (porta padrão)

## 📦 Dependências do Projeto

As dependências já estão configuradas no arquivo `.csproj`:

```xml
<PackageReference Include="Microsoft.AspNetCore.App" Version="8.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.3" />
<PackageReference Include="Microsoft.IdentityModel.Tokens" Version="7.0.3" />
```

### O que cada pacote faz:

- **Microsoft.AspNetCore.App** - Framework web
- **System.IdentityModel.Tokens.Jwt** - Criação e manipulação de JWT
- **Microsoft.IdentityModel.Tokens** - Validação de tokens

## 🔧 Configuração

### 1. appsettings.json

O arquivo já tem configuração padrão. Para desenvolvimento:

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

### 2. Variáveis de Ambiente (Production)

Configure em seu servidor:

```bash
# Linux/macOS
export Jwt__SecretKey="sua-chave-muito-segura"
export Jwt__ExpirationMinutes="60"

# Windows PowerShell
$env:Jwt__SecretKey = "sua-chave-muito-segura"
$env:Jwt__ExpirationMinutes = "60"

# Windows CMD
set Jwt__SecretKey=sua-chave-muito-segura
set Jwt__ExpirationMinutes=60
```

## 🏃 Executar a Aplicação

### Desenvolvimento

```bash
# Com reload automático de código
dotnet watch run --environment Development
```

### Production (Simulado)

```bash
dotnet run --environment Production
```

### Release Build

```bash
dotnet publish -c Release -o ./publish
cd ./publish
dotnet auth-system.dll
```

## 🐳 Usando Docker

### Build da Imagem

```bash
docker build -t auth-system:latest .
```

### Executar Container

```bash
docker run -p 5000:80 \
  -e Jwt__SecretKey="sua-chave-aqui" \
  auth-system:latest
```

### Usando Docker Compose

```bash
docker-compose up
```

## ✅ Verificar Instalação

### 1. Health Check

```bash
curl http://localhost:5000/health
```

### 2. Swagger/OpenAPI

Abra em seu navegador:
```
http://localhost:5000/swagger
```

### 3. Testar Endpoint

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"teste","email":"teste@example.com","password":"senha123","confirmPassword":"senha123"}'
```

## 🐛 Troubleshooting

### Erro: "dotnet: command not found"
```bash
# Verificar instalação do .NET
dotnet --version

# Instalar .NET 8.0 se não tiver
```

### Erro: "Port 5000 already in use"
```bash
# Linux/macOS - Encontrar processo
lsof -i :5000
kill -9 <PID>

# Windows PowerShell
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

### Erro: "Connection refused"
- Certificar que a aplicação está rodando
- Verificar configuração de firewall
- Verificar se está usando HTTPS (porta 5001) ou HTTP (5000)

### Erro: "Invalid Jwt Token"
- Verificar se a SecretKey está igual em todos os lugares
- Verificar se o token não expirou
- Certificar que o token está no header correto: `Authorization: Bearer <token>`

## 📊 Estrutura de Diretórios Criados

```
auth-system/
├── bin/                           (Saída da compilação)
├── obj/                           (Objetos compilados)
├── Controllers/
│   └── AuthController.cs
├── Models/
│   ├── User.cs
│   ├── LoginRequest.cs
│   ├── RegisterRequest.cs
│   └── AuthResponse.cs
├── Services/
│   ├── AuthService.cs
│   ├── JwtTokenService.cs
│   └── PasswordHashService.cs
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
└── SETUP.md (este arquivo)
```

## 🔐 Segurança - Próximas Ações

1. **Gerar nova SecretKey segura**
   ```bash
   # Linux/macOS
   openssl rand -base64 32
   
   # Windows PowerShell
   [Convert]::ToBase64String((1..32 | ForEach-Object {[byte](Get-Random -Maximum 256)}))
   ```

2. **Ativar HTTPS**
   - Certificado auto-assinado para dev
   - Certificado válido para produção (Let's Encrypt)

3. **Implementar Banco de Dados**
   - Remover armazenamento em memória
   - Usar Entity Framework Core
   - SQL Server ou PostgreSQL

4. **Adicionar Logging**
   - Serilog para logs estruturados
   - Rastreamento de tentativas de login

## 📚 Recursos Úteis

- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core/)
- [JWT.IO](https://jwt.io/) - Decodificar tokens
- [Azure App Service](https://azure.microsoft.com) - Deploy
- [Postman](https://www.postman.com/) - Testar API

## ❓ Perguntas Frequentes

**P: Como mudar a porta?**  
R: Edite `Properties/launchSettings.json` ou use `--urls` flag

**P: Como conectar um banco de dados?**  
R: Adicione Entity Framework Core e crie um `DbContext`

**P: Como adicionar mais roles/permissões?**  
R: Estenda o modelo `User` e adicione claims no JWT

---

**Pronto!** Seu sistema de autenticação deve estar funcionando. Consulte o [README.md](README.md) para mais detalhes.
