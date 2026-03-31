using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Conversation;

public record CreateConversationData(
    [MaxLength(100, ErrorMessage = "Title must not be longer than 100 characters")]
    string Title,
    List<int> participantIds
);
public record RemoveUserRequest(int UserId, int ConversationId);
public record AddUsersRequest(List<int> UserIds, int ConversationId);
public record SearchConversationRequest(int ConversationId, string Filter);
