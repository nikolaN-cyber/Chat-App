using Core.Types;
using Core.DTOs;

namespace Core.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginData request, CancellationToken cancellationToken);
        Task<ApiResponse<bool>> RegisterAsync(RegisterData request, CancellationToken cancellationToken);
    }
}
