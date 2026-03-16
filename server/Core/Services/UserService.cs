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

        public async Task<ApiResponse<UserResponse>> RegisterAsync(RegisterData request, CancellationToken cancellationToken)
        {
            var existingUser = await _context._users.AnyAsync(u => u.Email == request.Email || u.Username == request.Username, cancellationToken);
            if (existingUser)
            {
                return ApiResponse<UserResponse>.FailureResponse("User already exists");
            }
            var confirmPasswordCheck = request.Password == request.ConfirmPassword ? true : false;
            if (!confirmPasswordCheck)
            {
                return ApiResponse<UserResponse>.FailureResponse("Passwords do not match");
            }
            string hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);
            var newUser = new User
            {
                Username = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Age = request.Age,
                Email = request.Email,
                Password = hashedPassword
            };
            _context._users.Add(newUser);
            await _context.SaveChangesAsync(cancellationToken);
            return ApiResponse<UserResponse>.SuccessResponse(new UserResponse(
                newUser.Id,
                newUser.Username,
                newUser.FirstName,
                newUser.LastName,
                newUser.Age,
                newUser.Email
            ), "User successfully registered");
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

        public async Task<ApiResponse<string>> SendDirectMessage(MessageData request, CancellationToken cancellationToken)
        {
            int currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == 0)
            {
                return ApiResponse<string>.FailureResponse("Unauthorized");
            }
            var conversation = await _context._conversation.FirstOrDefaultAsync(c => c.Id == request.ConversationId);
            if (conversation == null)
            {
                return ApiResponse<string>.FailureResponse("Conversation does not exist");
            }

        }
    }
}
