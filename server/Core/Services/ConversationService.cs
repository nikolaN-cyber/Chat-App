using Core.DTOs.Conversation;
using Core.DTOs.Message;
using Core.Interfaces;
using Core.Types;
using Domain.Entities;
using Infrastructure.Contexts;
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
            try
            {
                int currentUserId = _currentUserService.GetCurrentUserId();
                if (currentUserId == 0) return ApiResponse<ConversationDetails>.FailureResponse("Unauthorized");

                var response = await _context._conversation
                    .AsNoTracking()
                    .Where(c => c.Id == conversationId)
                    .Select(c => new ConversationDetails(
                        c.Id,
                        c.Messages
                            .OrderBy(m => m.CreatedAt)
                            .Select(m => new MessageResponse(
                                m.Author != null ? (m.Author.Username ?? "Unknown") : "Unknown",
                                m.Content,
                                m.CreatedAt
                            )).ToList(),
                        c.Participants.Select( p => new ParticipantNames(p.User.Username, p.UserId)).ToList(),
                        c.AdminId
                    ))
                    .FirstOrDefaultAsync(cancellationToken);

                if (response == null) return ApiResponse<ConversationDetails>.FailureResponse("Conversation does not exist");

                return ApiResponse<ConversationDetails>.SuccessResponse(response);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Upit za konverzaciju {conversationId} je otkazan od strane klijenta.");
                return ApiResponse<ConversationDetails>.FailureResponse("Request was cancelled by user.");
            }
            catch (Exception ex)
            {
                var detailedError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Console.WriteLine($"Kritična greška: {detailedError}");
                return ApiResponse<ConversationDetails>.FailureResponse($"Server error: {ex.Message}");
            }
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
                        c.IsGroup,
                        allParticipantIds,
                        c.Participants.Select(p => p.User.Username).ToList(),
                        c.Participants.Where(p => p.UserId != currentUserId).Select(p => p.User.PhotoUrl).FirstOrDefault()
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
                .Select(u => new { u.Id, u.Username, u.FirstName, u.LastName, u.PhotoUrl })
                .ToListAsync(cancellationToken);

            var otherUser = participantData.FirstOrDefault(u => u.Id != currentUserId);

            var response = new ConversationResponse(
                Id: newConversation.Id,
                Title: isGroup ? newConversation.Title : otherUser != null ? $"{otherUser.FirstName} {otherUser.LastName}".Trim() : "Private Chat",
                IsGroup: newConversation.IsGroup,
                ParticipantIds: allParticipantIds,
                ParticipantNames: participantData.Select(u => u.Username).ToList(),
                PhotoUrl: !isGroup && otherUser != null ? otherUser.PhotoUrl : null
            );

            return ApiResponse<ConversationResponse>.SuccessResponse(response, isGroup ? "Grupa kreirana" : "Privatni čet kreiran");
        }

        public async Task<ApiResponse<List<ConversationResponse>>> GetUserConversationsAsync(CancellationToken cancellationToken)
        {
            try
            {
                int currentUserId = _currentUserService.GetCurrentUserId();
                if (currentUserId == 0) return ApiResponse<List<ConversationResponse>>.FailureResponse("Unauthorized");

                var query = _context._conversation
                    .AsNoTracking()
                    .Where(c => c.Participants.Any(p => p.UserId == currentUserId))
                    .Select(c => new ConversationResponse
                    (
                        c.Id,
                        c.IsGroup
                            ? (c.Title ?? "Group Chat")
                            : (c.Participants
                                .Where(p => p.UserId != currentUserId)
                                .Select(p => p.User != null
                                    ? (p.User.FirstName + " " + p.User.LastName).Trim()
                                    : "Private Chat")
                                .FirstOrDefault() ?? "Private Chat"),
                        c.IsGroup,
                        c.Participants.Select(p => p.UserId).ToList(),
                        c.Participants.Select(p => p.User != null
                            ? (p.User.FirstName ?? "User")
                            : "Unknown").ToList(),
                        !c.IsGroup
                            ? c.Participants
                                .Where(p => p.UserId != currentUserId)
                                .Select(p => p.User.PhotoUrl)
                                .FirstOrDefault()
                            : null
                    ));

                var conversations = await query.ToListAsync(cancellationToken);
                return ApiResponse<List<ConversationResponse>>.SuccessResponse(conversations);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message;
                return ApiResponse<List<ConversationResponse>>.FailureResponse($"Greška: {ex.Message}. Detalji: {innerMessage}");
            }
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

        public async Task<ApiResponse<bool>> RemoveUserAsync(RemoveUserRequest request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0)
            {
                return ApiResponse<bool>.FailureResponse("Unauthorized");
            }
            var conversation = await _context._conversation.FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);
            if (conversation == null)
            {
                return ApiResponse<bool>.FailureResponse("Conversation with this id does not exist");
            }
            if (conversation.AdminId != currentUserId)
            {
                return ApiResponse<bool>.FailureResponse("Unauthorized action");
            }
            var participant = await _context._participations.FirstOrDefaultAsync(p => p.ConversationId == request.ConversationId && p.UserId == request.UserId, cancellationToken);
            if (participant == null)
            {
                return ApiResponse<bool>.FailureResponse("User is not participating in this conversation");
            }
            _context._participations.Remove(participant);
            await _context.SaveChangesAsync(cancellationToken);
            return ApiResponse<bool>.SuccessResponse(true);
        }

        public async Task<ApiResponse<List<ParticipantNames>>> AddUserAsync(AddUsersRequest request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();

            var validation = await _context._conversation
                .Where(c => c.Id == request.ConversationId)
                .Select(c => new {
                    IsAdmin = c.AdminId == currentUserId,
                    ExistingIds = c.Participants.Select(p => p.UserId).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (validation == null) return ApiResponse<List<ParticipantNames>>.FailureResponse("Conversation not found");
            if (!validation.IsAdmin) return ApiResponse<List<ParticipantNames>>.FailureResponse("Only admin can manage members");

            var newIdsToAdd = request.UserIds.Except(validation.ExistingIds).ToList();

            if (!newIdsToAdd.Any())
                return ApiResponse<List<ParticipantNames>>.FailureResponse("All selected users are already members");

            var participantsData = await _context._users
                .Where(u => newIdsToAdd.Contains(u.Id))
                .Select(u => new ParticipantNames(u.Username, u.Id))
                .ToListAsync(cancellationToken);


            var participations = participantsData.Select(p => new Participation
            {
                ConversationId = request.ConversationId,
                UserId = p.userId,
                JoinedAt = DateTime.UtcNow
            });

            _context._participations.AddRange(participations);
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<List<ParticipantNames>>.SuccessResponse(participantsData);
        }
    }
}
