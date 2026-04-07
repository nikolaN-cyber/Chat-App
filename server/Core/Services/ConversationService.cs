using Core.DTOs.Conversation;
using Core.DTOs.Message;
using Core.Hubs;
using Core.Interfaces;
using Core.Types;
using Domain.Entities;
using Infrastructure.Contexts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Services
{
    public class ConversationService : IConversationService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ConversationService(AppDbContext context, ICurrentUserService currentUserService, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _currentUserService = currentUserService;
            _hubContext = hubContext;
        }

        public async Task<ApiResponse<ConversationDetails>> GetConversation(int conversationId, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0) throw new UnauthorizedAccessException("Unauthorized");

            var participation = await _context._participations.FirstOrDefaultAsync(p => p.UserId == currentUserId && p.ConversationId == conversationId, cancellationToken);

            if (participation == null)
                throw new UnauthorizedAccessException("You are not a participant of this conversation");

            var response = await _context._conversation
                .AsNoTracking()
                .Where(c => c.Id == conversationId)
                .Select(c => new ConversationDetails(
                    c.Id,
                    c.IsGroup
                        ? (c.Title ?? "Group Chat")
                        : (c.Participants.Count == 1
                            ? "My Notes"
                            : c.Participants
                                .Where(p => p.UserId != currentUserId)
                                .Select(p => p.User != null
                                    ? (p.User.FirstName + " " + p.User.LastName).Trim()
                                    : "Private Chat")
                                .FirstOrDefault() ?? "Private Chat"),
                    c.Messages
                    .Where(m => m.CreatedAt >= participation.JoinedAt)
                        .OrderBy(m => m.CreatedAt)
                        .Select(m => new MessageResponse(
                            m.Author != null ? (m.Author.Username ?? "Unknown") : "Unknown",
                            m.Content,
                            m.CreatedAt,
                            m.Author.PhotoUrl,
                            m.FileUrl,
                            m.FileType,
                            conversationId,
                            m.Type
                        )).ToList(),
                    c.Participants.Select( p => new ParticipantNames(p.User.Username, p.UserId, p.User.PhotoUrl)).ToList(),
                    c.AdminId
                ))
                .FirstOrDefaultAsync(cancellationToken);

            if (response == null) throw new KeyNotFoundException("Conversation does not exist");

            var actualLastMessageId = await _context._messages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.Id)
                .Select(m => m.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (actualLastMessageId > 0 && participation.LastReadMessageId < actualLastMessageId)
            {
                participation.LastReadMessageId = actualLastMessageId;
                _context._participations.Update(participation);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return ApiResponse<ConversationDetails>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<MessageResponse>>> GetMedia(int conversationId, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0) throw new UnauthorizedAccessException("Unauthorized");

            var participation = await _context._participations.FirstOrDefaultAsync(p => p.UserId == currentUserId && p.ConversationId == conversationId, cancellationToken);

            if (participation == null)
                throw new UnauthorizedAccessException("You are not a participant of this conversation");

            var media = await _context._messages.Where(m => m.ConversationId == conversationId && m.FileType != null && m.CreatedAt > participation.JoinedAt).OrderByDescending(m => m.CreatedAt).Select(m => 
            new MessageResponse(
                    m.Author.Username,
                    m.Content,
                    m.CreatedAt,
                    m.Author.PhotoUrl,
                    m.FileUrl,
                    m.FileType,
                    m.ConversationId,
                    m.Type
                )).ToListAsync(cancellationToken);

            if (media == null)
            {
                throw new ArgumentException("Media does not exist");
            }
            return ApiResponse<List<MessageResponse>>.SuccessResponse(media);
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

            bool isSelfChat = allParticipantIds.Count == 1;
            bool isGroup = allParticipantIds.Count > 2;

            if (!isGroup && !isSelfChat)
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
            else if (isSelfChat)
            {
                var existingNotes = await _context._conversation
                    .AsNoTracking()
                    .Where(c => !c.IsGroup && c.Participants.Count == 1 && c.Participants.Any(p => p.UserId == currentUserId))
                    .Select(c => new ConversationResponse(
                        c.Id,
                        c.Title ?? "My Notes (Saved)",
                        c.IsGroup,
                        0,
                        allParticipantIds,
                        c.Participants.Select(p => p.User.Username).ToList(),
                        c.Participants.Select(p => p.User.PhotoUrl).FirstOrDefault()
                    ))
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingNotes != null)
                    return ApiResponse<ConversationResponse>.SuccessResponse(existingNotes, "Učitan vaš Notes");
            }

            var newConversation = new Conversation
            {
                IsGroup = isGroup,
                Title = isGroup ? request.Title : (isSelfChat ? "My Notes" : null),
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
            string finalTitle = isGroup ? (newConversation.Title ?? "Unnamed group")
                               : (isSelfChat ? "My Notes"
                               : (otherUser != null ? $"{otherUser.FirstName} {otherUser.LastName}".Trim() : "Private Chat"));

            var response = new ConversationResponse(
                Id: newConversation.Id,
                Title: finalTitle,
                UnreadCount: 0,
                IsGroup: newConversation.IsGroup,
                ParticipantIds: allParticipantIds,
                ParticipantNames: participantsFromDb.Select(u => u.Username).ToList(),
                PhotoUrl: !isGroup && !isSelfChat && otherUser != null ? otherUser.PhotoUrl : null
            );

            var participantIdsAsStrings = allParticipantIds.Select(id => id.ToString()).ToList();

            await _hubContext.Clients.Users(participantIdsAsStrings)
                .SendAsync("CreateConversation", response, currentUserId);

            return ApiResponse<ConversationResponse>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<ConversationResponse>>> GetUserConversationsAsync(CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0) throw new UnauthorizedAccessException("Unauthorized");

            var conversations = await _context._conversation
                .AsNoTracking()
                .Where(c => c.Participants.Any(p => p.UserId == currentUserId))
                .Select(c => new
                {
                    Conversation = c,
                    LastReadId = c.Participants
                        .Where(p => p.UserId == currentUserId)
                        .Select(p => p.LastReadMessageId)
                        .FirstOrDefault(),
                    ParticipantDetails = c.Participants.Select(p => new { p.UserId, p.User.FirstName, p.User.LastName, p.User.PhotoUrl, p.User.Username }).ToList()
                })
                .ToListAsync(cancellationToken);

            var response = conversations.Select(x => {
                var c = x.Conversation;
                bool isSelfChat = x.ParticipantDetails.Count == 1 && x.ParticipantDetails.Any(p => p.UserId == currentUserId);
                var otherUser = x.ParticipantDetails.FirstOrDefault(p => p.UserId != currentUserId);

                return new ConversationResponse(
                    c.Id,
                    !c.IsGroup && isSelfChat ? "My Notes" :
                    c.IsGroup ? (c.Title ?? "Group Chat") :
                    (otherUser != null ? $"{otherUser.FirstName} {otherUser.LastName}".Trim() : "Private Chat"),
                    c.IsGroup,
                    _context._messages.Count(m => m.ConversationId == c.Id && m.Id > x.LastReadId),
                    x.ParticipantDetails.Select(p => p.UserId).ToList(),
                    x.ParticipantDetails.Select(p => p.Username).ToList(),
                    !c.IsGroup && otherUser != null ? otherUser.PhotoUrl : null
                );
            }).ToList();

            return ApiResponse<List<ConversationResponse>>.SuccessResponse(response);
        }

        public async Task<ApiResponse<bool>> DeleteConversationAsync(int ConversationId, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0)
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }
            var conversation = await _context._conversation.FirstOrDefaultAsync(c => c.Id == ConversationId, cancellationToken);
            if (conversation == null)
            {
                throw new ArgumentException("Conversation with this id does not exist");
            }
            if (conversation.AdminId != currentUserId)
            {
                throw new ArgumentException("Unauthorized access, only admin can delete");
            }
            _context._conversation.Remove(conversation);
            await _context.SaveChangesAsync(cancellationToken);

            string groupName = ConversationId.ToString();
            await _hubContext.Clients.Group(groupName).SendAsync("DeleteConversation", ConversationId, currentUserId);

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
            var participant = await _context._participations.Include(p => p.User).FirstOrDefaultAsync(p => p.ConversationId == request.ConversationId && p.UserId == request.UserId, cancellationToken);
            if (participant == null)
            {
                throw new UnauthorizedAccessException("User is not participant of this conversation");
            }

            var systemMessage = new Message
            {
                Content = $"{participant.User.Username} is removed from group",
                Type = MessageType.UserAdded,
                CreatedAt = DateTime.UtcNow,
                ConversationId = request.ConversationId
            };

            _context._messages.Add(systemMessage);
            await _context.SaveChangesAsync(cancellationToken);

            var messageToSend = new
            {
                id = systemMessage.Id,
                content = systemMessage.Content,
                type = systemMessage.Type,
                createdAt = systemMessage.CreatedAt,
                conversationId = systemMessage.ConversationId
            };

            await _hubContext.Clients.Group(request.ConversationId.ToString())
                .SendAsync("UserRemoved", new
                {
                    userId = request.UserId,
                    message = messageToSend
                });

            await Task.Delay(200);

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

            var systemMessage = new Message
            {
                Content = $"{participantsData[0].Username} is added to group",
                Type = MessageType.UserAdded,
                CreatedAt = DateTime.UtcNow,
                ConversationId = request.ConversationId
            };

            _context._messages.Add(systemMessage);
            await _context.SaveChangesAsync(cancellationToken);

            var participations = participantsData.Select(p => new Participation
            {
                ConversationId = request.ConversationId,
                UserId = p.userId,
                JoinedAt = DateTime.UtcNow,
                LastReadMessageId = systemMessage.Id
            });

            _context._participations.AddRange(participations);
            await _context.SaveChangesAsync(cancellationToken);

            await _hubContext.Clients.Group(request.ConversationId.ToString()).SendAsync("UserAdded", systemMessage);

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
                                request.ConversationId,
                                m.Type
                                )).ToListAsync(cancellationToken);

            return ApiResponse<List<MessageResponse>>.SuccessResponse(messages);          
        }

        public async Task<ApiResponse<bool>> DeleteChatHistory(DeleteConversationHistoryRequest request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0) throw new UnauthorizedAccessException("Unauthorized");

            var participation = await _context._participations.FirstOrDefaultAsync(p => p.UserId == currentUserId && p.ConversationId == request.ConversationId, cancellationToken);
            var lastMessageId = await _context._messages.Where(m => m.ConversationId == request.ConversationId).OrderByDescending(m => m.CreatedAt).Select(m => m.Id).FirstOrDefaultAsync(cancellationToken);

            if (participation == null)
            {
                throw new ArgumentException("Participation does not exist");
            }

            participation.JoinedAt = DateTime.UtcNow;
            participation.LastReadMessageId = lastMessageId;
            _context._participations.Update(participation);
            await _context.SaveChangesAsync(cancellationToken);
            return ApiResponse<bool>.SuccessResponse(true);
        }
    }
}
