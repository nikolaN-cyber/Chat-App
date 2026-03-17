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
        [HttpPost("get-or-create-private/{targetUserId}")]
        public async Task<IActionResult> GetOrCreateDirectConversation([FromRoute] int targetUserId, CancellationToken cancellationToken)
        {
            var response = await _conversationService.GetOrCreateConversation(targetUserId, cancellationToken);
            if (!response.Success)
            {
                if (response.Message == "Unauthorized") return Unauthorized(response);
                else return BadRequest(response);
            }
            return Ok(response);
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
    }
}
