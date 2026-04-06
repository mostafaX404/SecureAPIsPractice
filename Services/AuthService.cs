using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SecureAPIsPractice.Interfaces;
using SecureAPIsPractice.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
            var role = await _roleManager.RoleExistsAsync(model.UserId);
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
                ExpiresOn = jwtSecurityToken.ValidTo,
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
                expires: DateTime.Now.AddDays(_jwt.ExpirationInMinutes),
                signingCredentials: signingCredentials
            );

            return jwtSecurityToken;
        }
    }
}
