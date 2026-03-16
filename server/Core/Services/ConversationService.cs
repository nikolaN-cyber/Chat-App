using Core.Interfaces;
using Core.Types;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public class ConversationService : IConversationService
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserService _currentUserService;

        public ConversationService(AppDbContext context, CurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<int>> GetOrCreateConversation(int TargetUserId, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0)
            {
                return ApiResponse<int>.FailureResponse("Unauthorized");
            }
            var conversationId = await _context._conversation.Where(c => !c.IsGroup).Where(c => c.Participants.Any(p => p.UserId == currentUserId) && c.Participants.Any(p => p.UserId == TargetUserId))
                                                           .Select(c => c.Id).FirstOrDefaultAsync(cancellationToken);
            
            if (conversationId != 0)
            {
                return ApiResponse<int>.SuccessResponse(conversationId);
            }

            var newConversation = new Conversation
            {
                Title = "",
                IsGroup = false,
                Participants = new List<Participation>
                {
                     new Participation { UserId = currentUserId, JoinedAt = DateTime.UtcNow },
                     new Participation { UserId = TargetUserId, JoinedAt = DateTime.UtcNow }
                }
            };
            _context._conversation.Add(newConversation);
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<int>.SuccessResponse(newConversation.Id);
        }
    }
}
