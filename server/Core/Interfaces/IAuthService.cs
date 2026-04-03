using Core.DTOs.Auth;
using Core.Types;

namespace Core.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginData request, CancellationToken cancellationToken);
        Task<ApiResponse<bool>> RegisterAsync(RegisterData request, CancellationToken cancellationToken);
        Task<ApiResponse<LoginResponse>> LoginWithGoogleAsync(LoginGoogleData request, CancellationToken cancellationToken);
    }
}
