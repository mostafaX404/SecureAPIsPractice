using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using SecureAPIsPractice.Interfaces;
using SecureAPIsPractice.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SecureAPIsPractice.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtSettings _jwt;
        private readonly IMapper _mapper;
        public AuthService(UserManager<ApplicationUser> userManager , IMapper mapper,
            RoleManager<IdentityRole> roleManager,
            IOptions<JwtSettings> options) {

            _jwt = options.Value; 
            _userManager= userManager;
            _roleManager= roleManager;
            _mapper= mapper;
        }

        public async Task<string> AddRoleAsync(AddRoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            var role = await _roleManager.RoleExistsAsync(model.Role);
            if (user is null || role)
            {
                return "user not found";
            }

            if(await _userManager.IsInRoleAsync(user, model.Role))
            {
                return "User already in this role";
            }

            var result = await _userManager.AddToRoleAsync(user, model.Role);

            return result.Succeeded ? string.Empty : "Something went wrong";

        }

        public async Task<AuthModel> GetToken(RequestTokenModel model)
        {
            AuthModel authModel = new AuthModel();
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authModel.Message = "Email or password is incorrect";
                return authModel;
            }

            var jwtSecurityToken = await CreateJwtToken(user); 

            authModel.Username = user.UserName;
            authModel.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            authModel.Email = user.Email;
            authModel.Token =  new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.IsAuthenticated = true;
            authModel.ExpiresOn = jwtSecurityToken.ValidTo;

            if (user.RefreshTokens.Any(x => x.IsActive))
            {
                var refreshToken =  user.RefreshTokens.FirstOrDefault(x => x.IsActive);
                authModel.RefreshToken = refreshToken.Token ;
                authModel.RefreshTokenExpiration = refreshToken.ExpiresOn ;
            }
            else
            {
                var refreshToken = GenerateRefreshToken();
                authModel.RefreshToken = refreshToken.Token;
                authModel.RefreshTokenExpiration = refreshToken.ExpiresOn;
                user.RefreshTokens.Add(refreshToken);
                await _userManager.UpdateAsync(user);
            }

                return authModel;

        }

      

        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            //validate the unique valuew
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
            {
                return new AuthModel { Message = "Email is exist" };
            }
            if (await _userManager.FindByNameAsync(model.Username) is not null)
            {
                return new AuthModel { Message = "Username is exist" };
            }

            //maping the model to the form of the application user that user manager accept
            var user = _mapper.Map<ApplicationUser>(model);
            //var user = new ApplicationUser
            //{
            //    UserName = model.Username,
            //    Email = model.Email,
            //    FirstName = model.FirstName,
            //    LastName = model.LastName
            //};

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var error in result.Errors)
                {
                    errors += $"{error.Description}";

                }
                return new AuthModel { Message = errors };
            }
            await _userManager.AddToRoleAsync(user, "User");

            var jwtSecurityToken = await CreateJwtToken(user);

            return new AuthModel
            {
                Email = user.Email,
                //ExpiresOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Roles = new List<string> { "User" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Username = user.UserName
            };
        }

        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
            {
                roleClaims.Add(new Claim("roles", role));
            }

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim("uid", user.Id)
    }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwt.ExpirationInMinutes),
                signingCredentials: signingCredentials
            );

            return jwtSecurityToken;
        }

        private RefreshToken GenerateRefreshToken(int size = 32)
        {
            var randomNumber = new byte[size];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber) , 
          
                ExpiresOn = DateTime.UtcNow.AddDays(2)
            };
        }


        public  async Task<AuthModel> RefreshTokenAsync(string inputRefreshToken)
        {
            var authModel = new AuthModel();

            //get user that have this refresh token in his refreshTokens list
            var user = await _userManager.Users.SingleOrDefaultAsync(u=>u.RefreshTokens.Any(r=>r.Token == inputRefreshToken));
       
            //check if there is user has this token or it's inalid 
            if(user == null)
            {
                authModel.IsAuthenticated = false;
                authModel.Message = "Invalid refresh token";
                return authModel;
            }
            
            //get this refresh token

            var refreshToken = user.RefreshTokens.Single(t=>t.Token == inputRefreshToken);

            if (refreshToken.Isexpired)
            {
                authModel.IsAuthenticated = false;
                authModel.Message = "expired refresh token";
                return authModel;
            }

            //make the current refreshtoken revoked
            refreshToken.RevokedOn = DateTime.UtcNow;

            //create new refreshtoken and add it to refreshtokens user list and update the user
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);

            //create jwt Token
            var jwtToken = await CreateJwtToken(user);

            //prepare auth model
            authModel.IsAuthenticated = true; 
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            authModel.RefreshToken = newRefreshToken.Token;
            authModel.RefreshTokenExpiration = newRefreshToken.ExpiresOn;
            authModel.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            authModel.Email = user.Email;
            authModel.Username = user.UserName;


            return authModel;
        }

        public async Task<bool> RevokeRefreshToken(string token)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(x => x.RefreshTokens.Any(x => x.Token == token));

            if (user == null) return false;
            
            var refreshToken = user.RefreshTokens.Single(x=>x.Token == token);

            if (!refreshToken.IsActive) return false;

            refreshToken.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return true; 
        }
    }
}
