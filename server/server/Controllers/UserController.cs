using Azure.Core;
using Core.DTOs;
using Core.Hubs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IHubContext<ChatHub> _hubContext;
        public UserController(IUserService userService, IHubContext<ChatHub> hubContext)
        {
            _userService = userService;
            _hubContext = hubContext;
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

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] MessageData request, CancellationToken cancellationToken)
        {
            var result = await _userService.SendMessageAsync(request, cancellationToken);
            if (!result.Success)
            {
                if (result.Message == "Unauthorized") return Unauthorized(result);
                return BadRequest(result);
            }
            await _hubContext.Clients
                    .Group(request.ConversationId.ToString())
                    .SendAsync("ReceiveMessage", result.Data);
            return Ok(result.Data);
        }

        [HttpGet("search-by-username")]
        public async Task<IActionResult> FilterUsers([FromQuery] string? filter, CancellationToken cancellationToken)
        {
            var result = await _userService.FilterUsersByUsernameAsync(filter ?? "", cancellationToken);
            if (!result.Success)
            {
                if (result.Message == "Unauthorized") return Unauthorized(result);
                return BadRequest(result);
            }
            return Ok(result.Data);
        }
    }
}
