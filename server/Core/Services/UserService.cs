using Core.Interfaces;
using Core.Types;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Core.DTOs;

namespace Core.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        public UserService(AppDbContext context, ICurrentUserService currentUserService)
        { 
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<UserResponse>> EditAsync(EditUserData request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0)
            {
                return ApiResponse<UserResponse>.FailureResponse("Unauthorized");
            }
            var userToEdit = await _context._users.FindAsync(currentUserId, cancellationToken);
            if (userToEdit == null)
            {
                return ApiResponse<UserResponse>.FailureResponse("User with this Id: " + currentUserId + " does not exist");
            }

            if (userToEdit.Username != request.Username)
            {
                bool exists = await _context._users.AnyAsync(u => u.Username == request.Username, cancellationToken);
                if (exists) return ApiResponse<UserResponse>.FailureResponse("Username is already taken");
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
            var users = await _context._users.Select(u => new UserSummaryResponse(u.Id, u.Username)).ToListAsync(cancellationToken);
            return ApiResponse<List<UserSummaryResponse>>.SuccessResponse(users, "Users retreived successfully");
        }

        public async Task<ApiResponse<MessageResponse>> SendMessageAsync(MessageData request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0) return ApiResponse<MessageResponse>.FailureResponse("Unauthorized");

            var conversation = await _context._conversation
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

            if (conversation == null) return ApiResponse<MessageResponse>.FailureResponse("Conversation does not exist");

            if (!conversation.Participants.Any(p => p.UserId == currentUserId))
                return ApiResponse<MessageResponse>.FailureResponse("Niste učesnik ove konverzacije");

            var user = await _context._users.FirstAsync(u => u.Id == currentUserId);

            var message = new Message
            {
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                AuthorId = currentUserId,
                ConversationId = request.ConversationId
            };

            _context._messages.Add(message);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new MessageResponse(
                user.Username,
                message.Content,
                message.CreatedAt
            );

            return ApiResponse<MessageResponse>.SuccessResponse(response);
        }

        public async Task<ApiResponse<List<UserSummaryResponse>>> FilterUsersByUsernameAsync(string filter, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0)
            {
                return ApiResponse<List<UserSummaryResponse>>.FailureResponse("Unauthorized");
            }
            if (filter == "")
            {
                return ApiResponse<List<UserSummaryResponse>>.SuccessResponse(new List<UserSummaryResponse>());
            }
            var cleanFilter = filter.Trim();
            var users = await _context._users.Where(u => u.Id != currentUserId).Where(u => u.Username.StartsWith(cleanFilter)).Select(u => new UserSummaryResponse(u.Id, u.Username)).ToListAsync(cancellationToken);
            return ApiResponse<List<UserSummaryResponse>>.SuccessResponse(users);
        }
    }
}
