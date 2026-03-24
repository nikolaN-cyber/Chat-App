using Core.DTOs.User;
using Core.DTOs.Message;
using Core.Hubs;
using Core.Interfaces;
using Infrastructure.Contexts;
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
        private readonly AppDbContext _context;
        private readonly IPhotoService _photoService;
        public UserController(IUserService userService, IHubContext<ChatHub> hubContext, AppDbContext context, IPhotoService photoService)
        {
            _userService = userService;
            _hubContext = hubContext;
            _context = context;
            _photoService = photoService;
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

        [HttpPost("add-photo")]
        public async Task<IActionResult> AddPhoto(IFormFile file, CancellationToken cancellationToken)
        {
            try
            {
                var photoUrl = await _photoService.UpdateUserPhotoAsync(file, cancellationToken);
                return Ok(new { url = photoUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
