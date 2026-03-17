using Core.DTOs;
using Core.Interfaces;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService service)
        {
            _authService = service;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterData request, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginData request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result.Data);
        }
    }
}
