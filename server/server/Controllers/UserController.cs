using Core.Interfaces;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterData request, CancellationToken cancellationToken)
        {
            var result = await _userService.RegisterAsync(request, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPatch("edit")]
        public async Task<IActionResult> Edit([FromBody] EditUserData request, CancellationToken cancellationToken)
        {
            var result = await _userService.EditAsync(request, cancellationToken);
            if (!result.Success)
            {
                if (result.Message == "Unauthorized") return Unauthorized(result);
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _userService.GetAllUsersAsync(cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
