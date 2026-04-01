using Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Core.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor accessor)
        {
            _httpContextAccessor = accessor;
        }

        public int GetCurrentUserId()
        {
            var userStringId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userStringId))
            {
                return 0;
            }
            return int.Parse(userStringId);
        }
    }
}
