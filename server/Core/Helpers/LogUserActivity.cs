using Core.Interfaces;
using Infrastructure.Contexts;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        { 
            var resultContext = await next();

            if (resultContext.HttpContext.User.Identity?.IsAuthenticated != true) return;

            var currentUserService = resultContext.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
            var dbContext = resultContext.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var userId = currentUserService.GetCurrentUserId();

            if (userId != 0)
            {
                var user = await dbContext._users.FirstOrDefaultAsync(x => x.Id == userId);
                if (user != null)
                {
                    user.LastActive = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}