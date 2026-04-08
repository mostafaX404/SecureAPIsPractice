# SecureAPIsPractice

A secure ASP.NET Core Web API implementing **Authentication & Authorization** using JWT, Refresh Tokens, and role-based access.

---

## **Features**

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
  - Add roles to users.
  - Protect endpoints based on roles.
- **AutoMapper**
  - Map DTO models to `ApplicationUser` entity easily.
- **Secure Cookie Storage**
  - Refresh tokens stored in HttpOnly cookies for extra security.

---

## **Technologies & Libraries**

- **.NET 7 / ASP.NET Core Web API**
- **Entity Framework Core** (with Identity)
- **Microsoft.AspNetCore.Identity**
- **AutoMapper** for object mapping
- **System.IdentityModel.Tokens.Jwt** for JWT generation
- **Microsoft.Extensions.Options** for configuration
- **SQL Server / LocalDB** as the database

---

## **Getting Started**

1. **Clone the repository:**
   git clone https://github.com/<your-username>/SecureAPIsPractice.git
   cd SecureAPIsPractice

2. **Configure appsettings.json:**
   - Update the connection string:
   - Update JWT settings:
   
     
3. **Apply migrations to create the database:**
   dotnet ef database update

4. **Run the project:**
   dotnet run

5. **Test the API:**
   
     (There is collection of Postman APIs in project folder)
   - Register a user: POST /api/auth/register
   - Login: POST /api/auth/token
   - Add role: POST /api/auth/addrole
   - Refresh JWT: GET /api/auth/refreshtoken
   - Revoke token: POST /api/auth/revoketoken
