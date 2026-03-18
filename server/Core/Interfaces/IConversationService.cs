using Core.DTOs;
using Core.Types;

namespace Core.Interfaces
{
    public interface IConversationService
    {
        Task<ApiResponse<ConversationDetails>> GetOrCreateConversation(int TargetUserId, CancellationToken cancellationToken);
        Task<ApiResponse<int>> CreateGroupConversation(CreateConversationData request, CancellationToken cancellationToken);
        Task<ApiResponse<List<ConversationResponse>>> GetUserConversationsAsync(CancellationToken cancellationToken);
        Task<ApiResponse<bool>> DeleteConversationAsync(int Conversationid, CancellationToken cancellationToken);
    }
}
