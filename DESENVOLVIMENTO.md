# Guia de Desenvolvimento

## Configuração Inicial

### 1. Clonar o Repositório
```bash
git clone https://github.com/seu-usuario/auth-system.git
cd auth-system
```

### 2. Restaurar Dependências
```bash
dotnet restore
```

### 3. Executar Testes
```bash
dotnet test
```

### 4. Executar a Aplicação
```bash
# Development
dotnet run --environment Development

# Production (simulado)
dotnet run --environment Production
```

## Arquitetura

### Camadas

1. **Controllers** - Endpoints HTTP
2. **Services** - Lógica de negócio
3. **Models** - Entidades e DTOs
4. **Configuration** - Configuração da aplicação

### Fluxo de Autenticação

```
Cliente
  ↓
POST /api/auth/register ou /api/auth/login
  ↓
AuthController
  ↓
AuthService (validações)
  ↓
PasswordHashService (hash de senha)
  ↓
JwtTokenService (geração de token)
  ↓
Resposta com Token (se sucesso)
```

## Padrões de Código

- **Namespaces**: PascalCase (ex: `AuthSystem.Services`)
- **Classes**: PascalCase (ex: `AuthService`)
- **Interfaces**: Prefixo "I" + PascalCase (ex: `IAuthService`)
- **Métodos**: PascalCase (ex: `LoginAsync`)
- **Variáveis Privadas**: Underline + camelCase (ex: `_authService`)

## Melhorias Futuras

### Curto Prazo
- [ ] Adicionar testes unitários
- [ ] Implementar logging com Serilog
- [ ] Adicionar validação com FluentValidation
- [ ] Criar Database Service com Entity Framework

### Médio Prazo
- [ ] Implementar Refresh Tokens
- [ ] Adicionar autenticação social (Google, Facebook)
- [ ] Implementar 2FA
- [ ] Adicionar rate limiting

### Longo Prazo
- [ ] Containerizar com Docker
- [ ] Integrar com Azure ou AWS
- [ ] Implementar cache distribuído
- [ ] Adicionar análise de segurança

## Testes

### Executar Todos os Testes
```bash
dotnet test
```

### Executar Testes com Cobertura
```bash
dotnet test /p:CollectCoverage=true
```

### Teste de Carga
```bash
# Use ferramentas como Apache JMeter ou k6
```

## Segurança

### Checklist de Segurança

- [ ] JWT Secret configurado com chave forte
- [ ] HTTPS habilitado em produção
- [ ] CORS configurado restritivamente
- [ ] Validação de entrada em todos endpoints
- [ ] Rate limiting implementado
- [ ] Logging de atividades de autenticação
- [ ] Senhas com hash bcrypt (não SHA-256)
- [ ] Tokens com expiração curta

## Deployment

### Publicar para Produção
```bash
dotnet publish -c Release
```

### Configurar Variáveis de Ambiente
```bash
export Jwt__SecretKey="sua-chave-secreta-super-segura"
export Jwt__ExpirationMinutes="60"
```

### Docker
```bash
docker build -t auth-system:latest .
docker run -p 80:80 -p 443:443 auth-system:latest
```

## Troubleshooting

### Erro: "Secret key not found"
- Verificar `appsettings.json`
- Certificar que `Jwt:SecretKey` está configurado

### Erro: "Connection refused"
- Verificar se a aplicação está rodando
- Verificar a porta (padrão: 5000/5001)

### Erro: "Invalid token"
- Verificar se o token não expirou
- Verificar se a chave JWT está igual no servidor

## Documentação API

A documentação interativa está disponível em:
- **Swagger UI**: `http://localhost:5000/swagger`
- **Swagger JSON**: `http://localhost:5000/swagger/v1/swagger.json`

## Contato

Para dúvidas ou sugestões, abra uma issue no repositório.
