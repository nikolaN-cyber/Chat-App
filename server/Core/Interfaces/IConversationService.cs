using Core.Types;

namespace Core.Interfaces
{
    public interface IConversationService
    {
        Task<ApiResponse<int>> GetOrCreateConversation(int TargetUserId, CancellationToken cancellationToken);
    }
}
