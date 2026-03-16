using Core.Types;
using Core.DTOs;

namespace Core.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<UserResponse>> RegisterAsync(RegisterData request, CancellationToken cancellationToken);
        Task<ApiResponse<UserResponse>> EditAsync(EditUserData request, CancellationToken cancellationToken);
        Task<ApiResponse<List<UserSummaryResponse>>> GetAllUsersAsync(CancellationToken cancellationToken);
        Task<ApiResponse<string>> SendDirectMessage(MessageData request, CancellationToken cancellationToken);
    }
}
