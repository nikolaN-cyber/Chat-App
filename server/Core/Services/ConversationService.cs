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

        public async Task<ApiResponse<ConversationDetails>> GetOrCreateConversation(int targetUserId, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0) return ApiResponse<ConversationDetails>.FailureResponse("Unauthorized");

            var conversation = await _context._conversation
                .Include(c => c.Participants).ThenInclude(p => p.User)
                .Include(c => c.Messages).ThenInclude(m => m.Author)
                .FirstOrDefaultAsync(c => !c.IsGroup &&
                                          c.Participants.Any(p => p.UserId == currentUserId) &&
                                          c.Participants.Any(p => p.UserId == targetUserId), cancellationToken);

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    IsGroup = false,
                    Participants = new List<Participation>
                    {
                        new Participation { UserId = currentUserId, JoinedAt = DateTime.UtcNow },
                        new Participation { UserId = targetUserId, JoinedAt = DateTime.UtcNow }
                    },
                    AdminId = currentUserId,
                    Messages = new List<Message>()
                };
                _context._conversation.Add(conversation);
                await _context.SaveChangesAsync(cancellationToken);
            }


            var response = new ConversationDetails(
                Id: conversation.Id,
                Messages: (conversation.Messages ?? new List<Message>())
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new MessageResponse(
                        m.Author?.Username ?? "Unknown",
                        m.Content,
                        m.CreatedAt
                    )).ToList()
            );

            return ApiResponse<ConversationDetails>.SuccessResponse(response);
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
                    c.Participants.Select(p => p.User.Id).ToList(),
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
