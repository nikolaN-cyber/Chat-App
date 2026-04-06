using Core.DTOs.Conversation;
using Core.DTOs.Message;
using Core.Types;

namespace Core.Interfaces
{
    public interface IConversationService
    {
        Task<ApiResponse<ConversationDetails>> GetConversation(int conversationId, CancellationToken cancellationToken);
        Task<ApiResponse<ConversationResponse>> CreateConversation(CreateConversationData request, CancellationToken cancellationToken);
        Task<ApiResponse<List<ConversationResponse>>> GetUserConversationsAsync(CancellationToken cancellationToken);
        Task<ApiResponse<bool>> DeleteConversationAsync(int Conversationid, CancellationToken cancellationToken);
        Task<ApiResponse<bool>> RemoveUserAsync(RemoveUserRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<List<ParticipantNames>>> AddUserAsync(AddUsersRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<List<MessageResponse>>> SearchConversation(SearchConversationRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<bool>> DeleteChatHistory(DeleteConversationHistoryRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<List<MessageResponse>>> GetMedia(int conversationId, CancellationToken cancellationToken);
    }
}
