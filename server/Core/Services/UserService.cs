using Core.Interfaces;
using Core.Types;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Core.DTOs.User;
using Core.DTOs.Message;
using Core.Helpers;
using Infrastructure.Contexts;
using Core.DTOs.UserStatus;

namespace Core.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmailQueue _emailQueue;
        public UserService(AppDbContext context, ICurrentUserService currentUserService, IEmailQueue emailQueue)
        { 
            _context = context;
            _currentUserService = currentUserService;
            _emailQueue = emailQueue;
        }

        public async Task<ApiResponse<UserResponse>> EditAsync(EditUserData request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0)
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }
            var userToEdit = await _context._users.FindAsync(currentUserId, cancellationToken);
            if (userToEdit == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (userToEdit.Username != request.Username)
            {
                bool exists = await _context._users.AnyAsync(u => u.Username == request.Username, cancellationToken);
                if (exists) throw new ArgumentException("Username already taken");
            }

            userToEdit.Username = request.Username;
            userToEdit.FirstName = request.FirstName;
            userToEdit.LastName = request.LastName;
            userToEdit.Age = request.Age;

            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<UserResponse>.SuccessResponse(new UserResponse(
                userToEdit.Id,
                userToEdit.Username,
                userToEdit.FirstName,
                userToEdit.LastName,
                userToEdit.Age,
                userToEdit.Email
             ), "User successfuly edited");
        }

        public async Task<ApiResponse<List<UserSummaryResponse>>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            var users = await _context._users.Include(u => u.UserStatus).Select(u => new UserSummaryResponse
            (u.Id, 
            u.Username, 
            u.PhotoUrl,
            u.UserStatus != null ? new StatusResponse( u.UserStatus.Emoji, u.UserStatus.Status ) : null
            )).ToListAsync(cancellationToken);
            return ApiResponse<List<UserSummaryResponse>>.SuccessResponse(users, "Users retreived successfully");
        }

        public async Task<ApiResponse<MessageResponse>> SendMessageAsync(MessageData request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0) throw new UnauthorizedAccessException("Unauthorized");

            var conversation = await _context._conversation
                .Include(c => c.Participants)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

            if (conversation == null) throw new KeyNotFoundException("Conversation not found");

            var senderParticipation = conversation.Participants.FirstOrDefault(p => p.UserId == currentUserId);

            if (senderParticipation == null)
                throw new UnauthorizedAccessException("You are not part of this conversation");

            var sender = senderParticipation.User;

            var message = new Message
            {
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                FileUrl = request.FileUrl,
                FileType = request.FileType,
                AuthorId = currentUserId,
                ConversationId = request.ConversationId
            };

            _context._messages.Add(message);
            await _context.SaveChangesAsync(cancellationToken);
            conversation.LastMessageAddedId = message.Id;
            senderParticipation.LastReadMessageId = message.Id;
            await _context.SaveChangesAsync(cancellationToken);

            var recipients = conversation.Participants.Where(p => p.UserId != currentUserId && p.User != null).Select(p => p.User).ToList();

            foreach (var recipient in recipients)
            {
                if (recipient != null && recipient.LastActive < DateTime.UtcNow.AddMinutes(-10))
                {
                    var emailBody = EmailTemplate.GetNewMessageTemplate(
                            recipient.Username,
                            sender.Username,
                            message.Content
                    );
                    
                    _emailQueue.QueueEmail(new EmailMessage(
                            To: recipient.Email,
                            Subject: $"New message from {sender.Username}",
                            Body: emailBody
                    ));
                }
            }

            var response = new MessageResponse(
                sender.Username,
                message.Content,
                message.CreatedAt,
                sender.PhotoUrl,
                message.FileUrl,
                message.FileType,
                request.ConversationId,
                message.Type
            );
            return ApiResponse<MessageResponse>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<UserSummaryResponse>>> FilterUsersByUsernameAsync(string filter, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0)
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }
            if (filter == "")
            {
                return ApiResponse<List<UserSummaryResponse>>.SuccessResponse(new List<UserSummaryResponse>());
            }
            var cleanFilter = filter.Trim();
            var users = await _context._users.Include(u => u.UserStatus).Where(u => u.Id != currentUserId).Where(u => u.Username.StartsWith(cleanFilter)).Select(u => new UserSummaryResponse(
                u.Id,
                u.Username,
                u.PhotoUrl,
                (u.UserStatus != null && u.UserStatus.ExpiresAt > DateTime.UtcNow)
                    ? new StatusResponse(u.UserStatus.Emoji, u.UserStatus.Status)
                    : null
                )).ToListAsync(cancellationToken);
            return ApiResponse<List<UserSummaryResponse>>.SuccessResponse(users);
        }

        public async Task<ApiResponse<StatusResponse>> UpdateUserStatusAsync(AddStatus request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0) throw new UnauthorizedAccessException("Unauthorized");

            var existingStatus = await _context._userStatuses
                .FirstOrDefaultAsync(us => us.UserId == currentUserId, cancellationToken);

            if (string.IsNullOrEmpty(request.Status))
            {
                if (existingStatus != null)
                {
                    _context._userStatuses.Remove(existingStatus);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                return ApiResponse<StatusResponse>.SuccessResponse(null);
            }

            var validStatuses = new[] { "onvacation", "workingremotely" };
            if (!validStatuses.Contains(request.Status))
            {
                throw new ArgumentException("Invalid user status");
            }

            if (!request.ExpiresAt.HasValue || request.ExpiresAt.Value < DateTime.UtcNow)
            {
                throw new ArgumentException("Invalid or expired date");
            }

            if (existingStatus != null)
            {
                existingStatus.Emoji = request.Emoji!;
                existingStatus.Status = request.Status;
                existingStatus.ExpiresAt = request.ExpiresAt.Value;
                _context._userStatuses.Update(existingStatus);
            }
            else
            {
                var newStatus = new UserStatus
                {
                    Emoji = request.Emoji!,
                    Status = request.Status,
                    ExpiresAt = request.ExpiresAt.Value,
                    UserId = currentUserId,
                };
                _context._userStatuses.Add(newStatus);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return ApiResponse<StatusResponse>.SuccessResponse(new StatusResponse(request.Emoji, request.Status));
        }

        public async Task<ApiResponse<StatusResponse>> GetUserStatusAsync(CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0)
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }

            var status = await _context._userStatuses.FirstOrDefaultAsync(us => us.UserId == currentUserId, cancellationToken);
            if (status == null)
            {
                return ApiResponse<StatusResponse>.SuccessResponse(null);
            }

            var newStatusResponse = new StatusResponse(status.Emoji, status.Status);

            return ApiResponse<StatusResponse>.SuccessResponse(newStatusResponse);
        }
    }
}
