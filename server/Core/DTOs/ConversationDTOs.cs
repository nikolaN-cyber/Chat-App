using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public record CreateConversationData(
    [MaxLength(100, ErrorMessage = "Title must not be longer than 100 characters")]
    string Title, 
    List<int> participantIds
);

public record RemoveUserRequest(int UserId, int ConversationId);
public record AddUsersRequest(List<int> UserIds, int ConversationId);
public record ConversationResponse(int Id, string Title, bool IsGroup, List<int> ParticipantIds, List<string> ParticipantNames);
public record ConversationDetails(int Id, List<MessageResponse> Messages, List<ParticipantNames> Participants, int AdminId);
public record ParticipantNames(string Username, int userId);
