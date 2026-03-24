using Core.DTOs.Conversation;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _conversationService;

        public ConversationController(IConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        [Authorize]
        [HttpPost("get-private/{targetUserId}")]
        public async Task<IActionResult> GetDirectConversation([FromRoute] int targetUserId, CancellationToken cancellationToken)
        {
            var response = await _conversationService.GetConversation(targetUserId, cancellationToken);
            if (!response.Success)
            {
                if (response.Message == "Unauthorized") return Unauthorized(response);
                else return BadRequest(response);
            }
            return Ok(response.Data);
        }

        [Authorize]
        [HttpGet("get-all-user-conversations")]
        public async Task<IActionResult> GetUserConversations(CancellationToken cancellationToken)
        {
            var response = await _conversationService.GetUserConversationsAsync(cancellationToken);
            if (!response.Success)
            {
                if (response.Message == "Unauthorized") return Unauthorized(response);
                else return BadRequest(response);
            }
            return Ok(response.Data);
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationData request, CancellationToken cancellationToken)
        {
            var response = await _conversationService.CreateConversation(request, cancellationToken);
            if (!response.Success)
            {
                if (response.Message == "Unauthorized") return Unauthorized(response);
                else return BadRequest(response);
            }
            return Ok(response.Data);
        }

        [Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteConversation([FromRoute] int id, CancellationToken cancellationToken)
        {
            var response = await _conversationService.DeleteConversationAsync(id, cancellationToken);
            if (!response.Success)
            {
                if (response.Message == "Unauthorized") return Unauthorized(response);
                else return BadRequest(response);
            }
            return Ok(response.Data);
        }

        [Authorize]
        [HttpDelete("remove/{conversationId}/{userId}")]
        public async Task<IActionResult> RemoveUser([FromRoute] int conversationId, [FromRoute] int userId, CancellationToken cancellationToken)
        {
            var request = new RemoveUserRequest(
                userId,
                conversationId
             );
            var response = await _conversationService.RemoveUserAsync(request, cancellationToken);
            if (!response.Success)
            {
                if (response.Message == "Unauthorized") return Unauthorized(response);
                else return BadRequest(response);
            }
            return Ok(response.Data);
        }

        [Authorize]
        [HttpPost("add-users")]
        public async Task<IActionResult> AddUser([FromBody] AddUsersRequest request, CancellationToken cancellationToken)
        {
            var response = await _conversationService.AddUserAsync(request, cancellationToken);
            if (!response.Success)
            {
                if (response.Message == "Unauthorized") return Unauthorized(response);
                else return BadRequest(response);
            }
            return Ok(response.Data);
        }
    }
}
