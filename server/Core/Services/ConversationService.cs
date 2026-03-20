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

            var allParticipantIds = request.participantIds.Distinct().ToList();
            if (!allParticipantIds.Contains(currentUserId))
                allParticipantIds.Add(currentUserId);

            bool isGroup = allParticipantIds.Count > 2 || !string.IsNullOrWhiteSpace(request.Title);

            if (!isGroup)
            {
                int otherUserId = allParticipantIds.First(id => id != currentUserId);

                var existingChat = await _context._conversation
                    .Where(c => !c.IsGroup && c.Participants.Count == 2)
                    .Where(c => c.Participants.Any(p => p.UserId == currentUserId) &&
                                c.Participants.Any(p => p.UserId == otherUserId))
                    .Select(c => new ConversationResponse(
                        c.Id,
                        c.Participants.Where(p => p.UserId != currentUserId)
                                      .Select(p => p.User.FirstName + " " + p.User.LastName).FirstOrDefault() ?? "Private Chat",
                        allParticipantIds,
                        c.Participants.Select(p => p.User.Username).ToList()
                    ))
                    .FirstOrDefaultAsync(cancellationToken);
                if (existingChat != null)
                {
                    return ApiResponse<ConversationResponse>.SuccessResponse(existingChat, "Učitana postojeća konverzacija");
                }
            }

            var newConversation = new Conversation
            {
                IsGroup = isGroup,
                Title = isGroup ? request.Title : null,
                AdminId = currentUserId,
                Participants = allParticipantIds.Select(id => new Participation
                {
                    UserId = id,
                    JoinedAt = DateTime.UtcNow
                }).ToList()
            };

            try
            {
                _context._conversation.Add(newConversation);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return ApiResponse<ConversationResponse>.FailureResponse($"Greška pri upisu u bazu: {ex.Message}");
            }

            var participantData = await _context._users
                .Where(u => allParticipantIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Username, u.FirstName, u.LastName })
                .ToListAsync(cancellationToken);

            var otherUser = participantData.FirstOrDefault(u => u.Id != currentUserId);

            var response = new ConversationResponse(
                Id: newConversation.Id,
                Title: isGroup ? newConversation.Title : otherUser != null ? $"{otherUser.FirstName} {otherUser.LastName}".Trim() : "Private Chat",
                ParticipantIds: allParticipantIds,
                ParticipantNames: participantData.Select(u => u.Username).ToList()
            );

            return ApiResponse<ConversationResponse>.SuccessResponse(response, isGroup ? "Grupa kreirana" : "Privatni čet kreiran");
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
