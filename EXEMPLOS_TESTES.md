# Exemplos de Testes da API de Autenticação

Este arquivo contém exemplos de requisições HTTP para testar a API de autenticação.

## 1. Registrar um Novo Usuário

```
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "joao_silva",
  "email": "joao@example.com",
  "password": "Senha@123",
  "confirmPassword": "Senha@123"
}
```

**Respostas Esperadas:**

- ✅ Sucesso (201): Usuário registrado com sucesso
- ❌ Erro (400): Usuário já existe, senhas não coincidem, dados inválidos

## 2. Fazer Login

```
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "joao_silva",
  "password": "Senha@123"
}
```

**Resposta de Sucesso:**
```json
{
  "success": true,
  "message": "Login realizado com sucesso",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIiwibmFtZSI6Impv...",
  "user": {
    "id": 1,
    "username": "joao_silva",
    "email": "joao@example.com"
  }
}
```

**Copie o token retornado para usar nos próximos testes.**

## 3. Acessar Perfil Protegido

```
GET http://localhost:5000/api/auth/profile
Authorization: Bearer <seu_token_aqui>
```

**Exemplo Com Token Real:**
```
GET http://localhost:5000/api/auth/profile
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIn0...
```

**Respostas Esperadas:**

- ✅ Sucesso (200): Retorna dados do usuário
- ❌ Erro (401): Token ausente ou inválido
- ❌ Erro (404): Usuário não encontrado

## 4. Casos de Teste

### 4.1 Validação de Senha Fraca

```
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "teste",
  "email": "teste@example.com",
  "password": "123",
  "confirmPassword": "123"
}
```

**Resposta:**
```json
{
  "success": false,
  "message": "A senha deve ter no mínimo 6 caracteres"
}
```

### 4.2 Senhas Não Coincidem

```
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "teste",
  "email": "teste@example.com",
  "password": "senha123",
  "confirmPassword": "senha456"
}
```

**Resposta:**
```json
{
  "success": false,
  "message": "As senhas não coincidem"
}
```

### 4.3 Usuário Já Existe

```
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "joao_silva",
  "email": "outro@example.com",
  "password": "senha123",
  "confirmPassword": "senha123"
}
```

**Resposta:**
```json
{
  "success": false,
  "message": "Usuário já existe"
}
```

### 4.4 Credenciais Inválidas

```
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "joao_silva",
  "password": "senha_errada"
}
```

**Resposta:**
```json
{
  "success": false,
  "message": "Usuário ou senha inválidos"
}
```

## 5. Usando com cURL

### Registrar:
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"joao","email":"joao@example.com","password":"senha123","confirmPassword":"senha123"}'
```

### Login:
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"joao","password":"senha123"}'
```

### Perfil (com token):
```bash
curl -X GET http://localhost:5000/api/auth/profile \
  -H "Authorization: Bearer seu_token_aqui"
```

## 6. Variáveis de Ambiente para Testes

Para facilitar testes, defina estas variáveis no seu cliente HTTP:

| Variável | Valor |
|----------|-------|
| `base_url` | `http://localhost:5000` |
| `token` | Copiar do login |
| `username` | Seu nome de usuário de teste |
| `password` | Sua senha de teste |

## 7. Fluxo Completo de Teste

1. **POST** /api/auth/register - Registrar novo usuário
2. **POST** /api/auth/login - Fazer login (obter token)
3. **GET** /api/auth/profile - Acessar perfil protegido com o token

---

**Dica:** Use o Postman ou Insomnia para importar essas requisições facilmente!
