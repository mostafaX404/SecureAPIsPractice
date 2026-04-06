using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecureAPIsPractice.Interfaces;
using SecureAPIsPractice.Models;
using SecureAPIsPractice.Services;

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

            return Ok(result);

        }

        [HttpPost("addrole")]
        public async Task<IActionResult> AddRoleAsync(AddRoleModel addRoleModel)
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
    }
}
