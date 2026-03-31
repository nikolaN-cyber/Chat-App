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
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0) throw new UnauthorizedAccessException("Unauthorized");

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
                            m.CreatedAt,
                            m.Author.PhotoUrl,
                            m.FileUrl,
                            m.FileType,
                            conversationId
                        )).ToList(),
                    c.Participants.Select( p => new ParticipantNames(p.User.Username, p.UserId, p.User.PhotoUrl)).ToList(),
                    c.AdminId
                ))
                .FirstOrDefaultAsync(cancellationToken);

            if (response == null) throw new KeyNotFoundException("Conversation does not exist");

            var lastMessageId = await _context._messages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.Id)
                .Select(m => m.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastMessageId > 0)
            {
                var participation = await _context._participations
                    .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == currentUserId, cancellationToken);

                if (participation != null && participation.LastReadMessageId < lastMessageId)
                {
                    participation.LastReadMessageId = lastMessageId;
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            return ApiResponse<ConversationDetails>.SuccessResponse(response);
        }

        public async Task<ApiResponse<ConversationResponse>> CreateConversation(CreateConversationData request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0) throw new UnauthorizedAccessException("Unauthorized");

            var allParticipantIds = request.participantIds.Distinct().ToList();
            if (!allParticipantIds.Contains(currentUserId))
                allParticipantIds.Add(currentUserId);

            var participantsFromDb = await _context._users
                .AsNoTracking()
                .Where(u => allParticipantIds.Contains(u.Id))
                .ToListAsync(cancellationToken);

            if (participantsFromDb.Count != allParticipantIds.Count)
                throw new ArgumentException("One or more participants do not exist");

            bool isGroup = allParticipantIds.Count > 2 || !string.IsNullOrWhiteSpace(request.Title);

            if (!isGroup)
            {
                int otherUserId = allParticipantIds.First(id => id != currentUserId);

                var existingChat = await _context._conversation
                    .AsNoTracking()
                    .Where(c => !c.IsGroup && c.Participants.Count == 2)
                    .Where(c => c.Participants.Any(p => p.UserId == currentUserId) &&
                                c.Participants.Any(p => p.UserId == otherUserId))
                    .Select(c => new ConversationResponse(
                        c.Id,
                        c.Participants.Where(p => p.UserId != currentUserId)
                                      .Select(p => p.User.FirstName + " " + p.User.LastName).FirstOrDefault() ?? "Private Chat",
                        
                        c.IsGroup,
                        c.Messages.Count(m => m.Id > c.Participants
                            .Where(p => p.UserId == currentUserId)
                            .Select(p => p.LastReadMessageId)
                            .FirstOrDefault()),
                        allParticipantIds,
                        c.Participants.Select(p => p.User.Username).ToList(),
                        c.Participants.Where(p => p.UserId != currentUserId).Select(p => p.User.PhotoUrl).FirstOrDefault()
                    ))
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingChat != null)
                    return ApiResponse<ConversationResponse>.SuccessResponse(existingChat, "Učitana postojeća konverzacija");
            }

            var newConversation = new Conversation
            {
                IsGroup = isGroup,
                Title = isGroup ? request.Title : null,
                AdminId = currentUserId,
                Participants = allParticipantIds.Select(id => new Participation
                {
                    UserId = id,
                    JoinedAt = DateTime.UtcNow,
                    LastReadMessageId = 0
                }).ToList()
            };

            _context._conversation.Add(newConversation);
            await _context.SaveChangesAsync(cancellationToken);

            var otherUser = participantsFromDb.FirstOrDefault(u => u.Id != currentUserId);

            var response = new ConversationResponse(
                Id: newConversation.Id,
                Title: isGroup ? newConversation.Title : otherUser != null ? $"{otherUser.FirstName} {otherUser.LastName}".Trim() : "Private Chat",
                UnreadCount: 0, // Za novu konverzaciju je uvek 0
                IsGroup: newConversation.IsGroup,
                ParticipantIds: allParticipantIds,
                ParticipantNames: participantsFromDb.Select(u => u.Username).ToList(),
                PhotoUrl: !isGroup && otherUser != null ? otherUser.PhotoUrl : null
            );

            return ApiResponse<ConversationResponse>.SuccessResponse(response, isGroup ? "Grupa kreirana" : "Privatni čet kreiran");
        }

        public async Task<ApiResponse<List<ConversationResponse>>> GetUserConversationsAsync(CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0) throw new UnauthorizedAccessException("Unauthorized");

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
                    c.Messages.Count(m => m.Id > c.Participants
                        .Where(p => p.UserId == currentUserId)
                        .Select(p => p.LastReadMessageId)
                        .FirstOrDefault()),
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
                throw new UnauthorizedAccessException("Unauthorized");
            }
            var conversation = await _context._conversation.FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);
            if (conversation == null)
            {
                throw new KeyNotFoundException("Conversation not found");
            }
            if (conversation.AdminId != currentUserId)
            {
                throw new UnauthorizedAccessException("Only admin can remove members");
            }
            var participant = await _context._participations.FirstOrDefaultAsync(p => p.ConversationId == request.ConversationId && p.UserId == request.UserId, cancellationToken);
            if (participant == null)
            {
                throw new UnauthorizedAccessException("User is not participant of this conversation");
            }
            _context._participations.Remove(participant);
            await _context.SaveChangesAsync(cancellationToken);
            return ApiResponse<bool>.SuccessResponse(true);
        }

        public async Task<ApiResponse<List<ParticipantNames>>> AddUserAsync(AddUsersRequest request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();

            if (currentUserId == 0)
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }

            var validation = await _context._conversation
                .Where(c => c.Id == request.ConversationId)
                .Select(c => new {
                    IsAdmin = c.AdminId == currentUserId,
                    ExistingIds = c.Participants.Select(p => p.UserId).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (validation == null) throw new KeyNotFoundException("Conversationn not found");
            if (!validation.IsAdmin) throw new UnauthorizedAccessException("Only admin can add members");

            var newIdsToAdd = request.UserIds.Except(validation.ExistingIds).ToList();

            if (!newIdsToAdd.Any())
                throw new ArgumentException("These users are already members");

            var participantsData = await _context._users
                .Where(u => newIdsToAdd.Contains(u.Id))
                .Select(u => new ParticipantNames(u.Username, u.Id, u.PhotoUrl))
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

        public async Task<ApiResponse<List<MessageResponse>>> SearchConversation(SearchConversationRequest request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();

            if (currentUserId == 0)
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }
            if (request.Filter == "")
            {
                return ApiResponse<List<MessageResponse>>.SuccessResponse(new List<MessageResponse>());
            }

            var messages = await _context._messages.Where(m => m.ConversationId == request.ConversationId && m.Content.ToLower().Contains(request.Filter.ToLower()))
                     .OrderByDescending(m => m.CreatedAt).Select(m => new MessageResponse(
                                m.Author != null ? (m.Author.Username ?? "Unknown") : "Unknown",
                                m.Content,
                                m.CreatedAt,
                                m.Author.PhotoUrl,
                                m.FileUrl,
                                m.FileType,
                                request.ConversationId
                                )).ToListAsync(cancellationToken);

            return ApiResponse<List<MessageResponse>>.SuccessResponse(messages);          
        }
    }
}
