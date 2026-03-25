using Core.Types;
using Core.DTOs.Message;
using Core.DTOs.User;
using Core.DTOs.UserStatus;

namespace Core.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<UserResponse>> EditAsync(EditUserData request, CancellationToken cancellationToken);
        Task<ApiResponse<List<UserSummaryResponse>>> GetAllUsersAsync(CancellationToken cancellationToken);
        Task<ApiResponse<MessageResponse>> SendMessageAsync(MessageData request, CancellationToken cancellationToken);
        Task<ApiResponse<List<UserSummaryResponse>>> FilterUsersByUsernameAsync(string filter, CancellationToken cancellationToken);
        Task<ApiResponse<StatusResponse>> UpdateUserStatusAsync(AddStatus request, CancellationToken cancellationToken);
        Task<ApiResponse<StatusResponse>> GetUserStatusAsync(CancellationToken cancellationToken);
    }
}
