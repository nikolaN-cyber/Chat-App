using Core.Types;
using Core.DTOs.Message;
using Core.DTOs.User;

namespace Core.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<UserResponse>> EditAsync(EditUserData request, CancellationToken cancellationToken);
        Task<ApiResponse<List<UserSummaryResponse>>> GetAllUsersAsync(CancellationToken cancellationToken);
        Task<ApiResponse<MessageResponse>> SendMessageAsync(MessageData request, CancellationToken cancellationToken);
        Task<ApiResponse<List<UserSummaryResponse>>> FilterUsersByUsernameAsync(string filter, CancellationToken cancellationToken);
    }
}
