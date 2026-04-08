using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SecureAPIsPractice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class TestRolesController : ControllerBase
    {

        [HttpGet("adminsonly")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminsOnly()
        {
            var name = User.Identity?.Name;
            return Ok($"Hi admin : {name}");
        }


        [HttpGet("usersonly")]
        [Authorize(Roles =  "User")]
        public IActionResult UsersOnly()
        {
            var name = User.Identity?.Name;
            return Ok($"Hi user : {name}");
        }
    }
}
