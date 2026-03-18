using Core.Types;
using Core.DTOs;

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
