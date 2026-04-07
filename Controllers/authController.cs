using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using SecureAPIsPractice.Interfaces;
using SecureAPIsPractice.Models;
using SecureAPIsPractice.Services;
using System.Net;

namespace SecureAPIsPractice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class authController : ControllerBase
    {
        private readonly IAuthService _authService;

        public authController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpPost("register")]

        public async Task<IActionResult> Register(RegisterModel registerModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(registerModel);

            if (!result.IsAuthenticated)
            {
                return BadRequest(result.Message);
            }

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);


            return Ok(result);

        }

        [HttpPost("token")]
        public async Task<IActionResult> GetTokenAsync(RequestTokenModel registerModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.GetToken(registerModel);

            if (!result.IsAuthenticated)
            {
                return BadRequest(result.Message);
            }

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);
            }

            return Ok(result);

        }

        [HttpPost("addrole")]
        public async Task<IActionResult> AddRoleAsync([FromBody] AddRoleModel addRoleModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.AddRoleAsync(addRoleModel);

            if (!string.IsNullOrEmpty(result))
            {
                return BadRequest(result);
            }

            return Ok(addRoleModel);

        }

        [HttpGet("refreshtoken")]
        public async Task<IActionResult> RefreshToken()
        {
            var token = Request.Cookies["refreshToken"];

            var result  = await _authService.RefreshTokenAsync(token);

            if (!result.IsAuthenticated)
            {
                return BadRequest(result);
            }

            SetRefreshTokenInCookie(result.RefreshToken,result.RefreshTokenExpiration);

            return Ok(result);
        }

        [HttpPost("revoketoken")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenModel?  model)
        {
            
            model.Token = model.Token ??  Request.Cookies["refreshToken"];
            

            if (string.IsNullOrEmpty(model.Token))
            {
                return BadRequest("Model Is Required");
            }

            var decodedToken = WebUtility.UrlDecode(model.Token);

            var result = await _authService.RevokeRefreshToken(decodedToken);

            if (!result)
            {
                return BadRequest("Token is invalid ");
            }

            return Ok("Token is revoked successfully");
        }

        private void SetRefreshTokenInCookie(string refreshToken, DateTime expiresOn)
        {
            CookieOptions options = new CookieOptions
            {
                HttpOnly = true,
                Expires = expiresOn.ToLocalTime()
            };

            Response.Cookies.Append("refreshToken",refreshToken, options);
        }
    
    
        
    }
}
