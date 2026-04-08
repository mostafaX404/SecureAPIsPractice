# SecureAPIsPractice
A secure ASP.NET Core Web API implementing **Authentication & Authorization** using JWT, Refresh Tokens, and role-based access.

---

## Features

- **User Registration & Login**
  - Register new users with email, username, password.
  - Login with email/password to get JWT access token.

- **JWT Authentication**
  - Generate short-lived JWT tokens for secure API access.
  - Include user roles and claims in JWT.

- **Refresh Tokens**
  - Issue refresh tokens for renewing access tokens without re-login.
  - Revoke refresh tokens when necessary.

- **Role-Based Authorization**
  - Assign roles to users via a dedicated endpoint.
  - Protect endpoints using `[Authorize(Roles = "RoleName")]`.
  - Roles are embedded inside the JWT as standard claims.
  - Supports multiple roles per user.
  - Returns `403 Forbidden` for unauthorized role access.

- **AutoMapper**
  - Map DTO models to `ApplicationUser` entity easily.

- **Secure Cookie Storage**
  - Refresh tokens stored in HttpOnly cookies for extra security.

---

## Technologies & Libraries

- .NET 7 / ASP.NET Core Web API
- Entity Framework Core (with Identity)
- Microsoft.AspNetCore.Identity
- AutoMapper for object mapping
- System.IdentityModel.Tokens.Jwt for JWT generation
- Microsoft.Extensions.Options for configuration
- SQL Server / LocalDB as the database

---

## Getting Started

1. **Clone the repository:**
```bash
git clone https://github.com/<your-username>/SecureAPIsPractice.git
cd SecureAPIsPractice
```

2. **Configure appsettings.json:**
   - Update the connection string.
   - Update JWT settings.

3. **Apply migrations:**
```bash
dotnet ef database update
```

4. **Run the project:**
```bash
dotnet run
```

5. **Test the API:**

*(There is a collection of Postman APIs in the project folder)*

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/token` | Login and get JWT |
| POST | `/api/auth/addrole` | Assign a role to a user |
| GET | `/api/auth/refreshtoken` | Refresh JWT using cookie |
| POST | `/api/auth/revoketoken` | Revoke a refresh token |
| GET | `/api/testroles/adminsonly` | Accessible by Admin role only |
| GET | `/api/testroles/usersonly` | Accessible by User role only |

---

## Role-Based Authorization — How It Works

### 1. Assign a Role to a User
Send a `POST` request to `/api/auth/addrole`:
```json
{
  "userId": "your-user-id",
  "role": "Admin"
}
```

### 2. Login and Get JWT
Send a `POST` request to `/api/auth/token`. The returned JWT will contain the assigned role as a claim:
```json
{
  "roles": "Admin"
}
```

### 3. Access a Protected Endpoint
Pass the JWT in the `Authorization` header

Endpoints are protected using the `[Authorize]` attribute:
```csharp
[Authorize(Roles = "Admin")]
public IActionResult AdminsOnly() { ... }

[Authorize(Roles = "User")]
public IActionResult UsersOnly() { ... }
```

### 4. Available Roles

| Role | Description |
|------|-------------|
| `User` | Assigned by default on registration |
| `Admin` | Assigned manually via `/api/auth/addrole` |

