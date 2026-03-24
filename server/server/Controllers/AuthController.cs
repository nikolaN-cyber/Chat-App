using Core.DTOs;
using Core.Helpers;
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
        private readonly IEmailQueue _emailQueue;
        public AuthController(IAuthService service, IEmailQueue emailQueue)
        {
            _authService = service;
            _emailQueue = emailQueue;
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
            try
            {
                var emailBody = EmailTemplate.GetWelcomeTemplate(request.Username);
                var welcomeEmail = new EmailMessage(
                    To: request.Email,
                    Subject: "Welcome!",
                    Body: emailBody
                );
                _emailQueue.QueueEmail(welcomeEmail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri dodavanju mejla u red: {ex.Message}");
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
