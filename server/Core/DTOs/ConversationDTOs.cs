using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public record CreateConversationData(
    [MaxLength(100, ErrorMessage = "Title must not be longer than 100 characters")]
    string Title, 
    List<int> participantIds
);
public record ConversationResponse(int Id, string Title, bool IsGroup, List<int> ParticipantIds, List<string> ParticipantNames);
public record ConversationDetails(int Id, List<MessageResponse> Messages);
