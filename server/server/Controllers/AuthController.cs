using Core.DTOs.Auth;
using Core.DTOs.Message;
using Core.Helpers;
using Core.Interfaces;
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
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthService service, IEmailQueue emailQueue, ILogger<AuthController> logger)
        {
            _authService = service;
            _emailQueue = emailQueue;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterData request, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);

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
            catch
            {
                _logger.LogWarning("Email queue is currently not working.");
            }
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginData request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            return Ok(result);
        }
    }
}
