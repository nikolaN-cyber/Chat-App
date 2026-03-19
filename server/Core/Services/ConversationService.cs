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

        public async Task<ApiResponse<ConversationDetails>> GetConversation(int conversationId, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0) return ApiResponse<ConversationDetails>.FailureResponse("Unauthorized");
            var response = await _context._conversation
                .Where(c => c.Id == conversationId)
                .Select(c => new ConversationDetails(
                    c.Id,
                    c.Messages
                        .OrderBy(m => m.CreatedAt)
                        .Select(m => new MessageResponse(
                            m.Author.Username ?? "Unknown",
                            m.Content,
                            m.CreatedAt
                        )).ToList()
                ))
                .FirstOrDefaultAsync(cancellationToken);

            if (response == null)
            {
                return ApiResponse<ConversationDetails>.FailureResponse("Conversation does not exist");
            }

            return ApiResponse<ConversationDetails>.SuccessResponse(response);
        }

        public async Task<ApiResponse<ConversationResponse>> CreateConversation(CreateConversationData request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0) return ApiResponse<ConversationResponse>.FailureResponse("Unauthorized");

            var allParticipantIds = request.participantIds.ToList();
            if (!allParticipantIds.Contains(currentUserId))
                allParticipantIds.Add(currentUserId);

            if (allParticipantIds.Count == 2)
            {
                var existingChat = await _context._conversation
                    .Where(c => !c.IsGroup)
                    .Where(c => c.Participants.All(p => allParticipantIds.Contains(p.UserId))
                             && c.Participants.Count == 2)
                    .Select(c => new ConversationResponse(
                        c.Id,
                        c.Participants.Where(p => p.UserId != currentUserId)
                                     .Select(p => p.User.Username).FirstOrDefault() ?? "Private Chat",
                        allParticipantIds,
                        c.Participants.Select(p => p.User.Username).ToList()
                    ))
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingChat != null)
                {
                    return ApiResponse<ConversationResponse>.SuccessResponse(existingChat, "Existing conversation returned");
                }
            }

            var participants = allParticipantIds.Select(id => new Participation
            {
                UserId = id,
                JoinedAt = DateTime.UtcNow
            }).ToList();

            var newConversation = new Conversation
            {
                Title = request.Title,
                IsGroup = true,
                Participants = participants,
                AdminId = currentUserId
            };

            _context._conversation.Add(newConversation);
            await _context.SaveChangesAsync(cancellationToken);

            var participantData = await _context._users
                .Where(u => allParticipantIds.Contains(u.Id))
                .Select(u => u.Username)
                .ToListAsync(cancellationToken);

            var response = new ConversationResponse(
                Id: newConversation.Id,
                Title: newConversation.Title,
                ParticipantIds: allParticipantIds,
                ParticipantNames: participantData
            );

            return ApiResponse<ConversationResponse>.SuccessResponse(response, "Group chat created");
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
