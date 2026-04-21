# API Authentication Testing Examples

This file contains HTTP request examples for testing the authentication API.

## 1. Register New User

```
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass123",
  "confirmPassword": "SecurePass123"
}
```

**Expected Responses:**

- Success (200): User registered successfully
- Error (400): User already exists, passwords don't match, invalid data

## 2. Login

```
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "john_doe",
  "password": "SecurePass123"
}
```

**Success Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIiwibmFtZSI6ImpvaG4i...",
  "user": {
    "id": 1,
    "username": "john_doe",
    "email": "john@example.com"
  }
}
```

**Copy the returned token to use in the following tests.**

## 3. Access Protected Profile

```
GET http://localhost:5000/api/auth/profile
Authorization: Bearer <your_token_here>
```

**Example With Real Token:**
```
GET http://localhost:5000/api/auth/profile
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIn0...
```

**Expected Responses:**

- Success (200): Returns user data
- Error (401): Token missing or invalid
- Error (404): User not found

## 4. Test Cases

### 4.1 Weak Password Validation

```
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "test",
  "email": "test@example.com",
  "password": "123",
  "confirmPassword": "123"
}
```

**Response:**
```json
{
  "success": false,
  "message": "Password must have at least 8 characters"
}
```

### 4.2 Passwords Don't Match

```
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "test",
  "email": "test@example.com",
  "password": "password123",
  "confirmPassword": "password456"
}
```

**Response:**
```json
{
  "success": false,
  "message": "Passwords do not match"
}
```

### 4.3 User Already Exists

```
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "john_doe",
  "email": "another@example.com",
  "password": "password123",
  "confirmPassword": "password123"
}
```

**Response:**
```json
{
  "success": false,
  "message": "Username already exists"
}
```

### 4.4 Invalid Credentials

```
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "john_doe",
  "password": "wrong_password"
}
```

**Response:**
```json
{
  "success": false,
  "message": "Invalid credentials"
}
```

## 5. Using with cURL

### Register:
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"john","email":"john@example.com","password":"password123","confirmPassword":"password123"}'
```

### Login:
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"john","password":"password123"}'
```

### Profile (with token):
```bash
curl -X GET http://localhost:5000/api/auth/profile \
  -H "Authorization: Bearer your_token_here"
```

### Health Check:
```bash
curl -X GET http://localhost:5000/api/healthcheck
```

## 6. Environment Variables for Tests

To facilitate testing, set these variables in your HTTP client:

| Variable | Value |
|----------|-------|
| `base_url` | `http://localhost:5000` |
| `token` | Copy from login |
| `username` | Your test username |
| `password` | Your test password |

## 7. Complete Test Flow

1. **POST** /api/auth/register - Register new user
2. **POST** /api/auth/login - Login (obtain token)
3. **GET** /api/auth/profile - Access protected profile with token
4. **GET** /api/healthcheck - Check application health

## 8. Invalid Email Test

```
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "test",
  "email": "invalid-email",
  "password": "password123",
  "confirmPassword": "password123"
}
```

**Response:**
```json
{
  "success": false,
  "message": "Invalid email format"
}
```

## 9. Invalid Username Length

```
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "ab",
  "email": "test@example.com",
  "password": "password123",
  "confirmPassword": "password123"
}
```

**Response:**
```json
{
  "success": false,
  "message": "Username must be between 3 and 50 characters"
}
```

---

**Tip:** Use Postman or Insomnia to import these requests easily!
