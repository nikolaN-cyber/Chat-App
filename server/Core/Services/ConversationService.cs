using Core.DTOs;
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
        private readonly ICurrentUserService _currentUserService;

        public ConversationService(AppDbContext context, ICurrentUserService currentUserService)
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
                },
               AdminId = currentUserId
            };
            _context._conversation.Add(newConversation);
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<int>.SuccessResponse(newConversation.Id, "Private chat created");
        }

        public async Task<ApiResponse<int>> CreateGroupConversation(CreateConversationData request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0)
            {
                return ApiResponse<int>.FailureResponse("Unauthorized");
            }

            var participants = new List<Participation>();

            participants.Add(new Participation
            {
                UserId = currentUserId,
                JoinedAt = DateTime.Now
            });

            foreach (var userId in request.participantIds)
            {
                participants.Add(new Participation
                {
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow
                });
            }

            var newGroup = new Conversation
            {
                Title = request.Title,
                IsGroup = true,
                Participants = participants,
                AdminId = currentUserId
            };

            _context._conversation.Add(newGroup);

            await _context.SaveChangesAsync(cancellationToken);
            return ApiResponse<int>.SuccessResponse(newGroup.Id, "Group chat created");
        }

        public async Task<ApiResponse<List<ConversationResponse>>> GetUserConversationsAsync(CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0)
            {
                return ApiResponse<List<ConversationResponse>>.FailureResponse("Unauthorized");
            }
            var conversations = await _context._conversation
                .Where(c => c.Participants.Any(p => p.UserId == currentUserId))
                .Select(c => new ConversationResponse
                (
                    c.Id,
                    c.IsGroup ? c.Title : c.Participants.Where(p => p.UserId != currentUserId).Select(p => p.User.FirstName + " " + p.User.LastName).FirstOrDefault(),
                    c.Participants.Select(p => p.User.FirstName).ToList()
                )).ToListAsync(cancellationToken);
            return ApiResponse<List<ConversationResponse>>.SuccessResponse(conversations);
        }

        public async Task<ApiResponse<bool>> DeleteConversationAsync(int ConversationId, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0)
            {
                return ApiResponse<bool>.FailureResponse("Unauthorized");
            }
            var conversation = await _context._conversation.FirstOrDefaultAsync(c => c.Id == ConversationId, cancellationToken);
            if (conversation == null)
            {
                return ApiResponse<bool>.FailureResponse("Conersation with this id does not exist");
            }
            if (conversation.AdminId != currentUserId)
            {
                return ApiResponse<bool>.FailureResponse("Unauthorized action");
            }
            _context._conversation.Remove(conversation);
            await _context.SaveChangesAsync(cancellationToken);
            return ApiResponse<bool>.SuccessResponse(true);
        }
    }
}
