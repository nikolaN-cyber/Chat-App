using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public record CreateConversationData(
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(100, ErrorMessage = "Title must not be longer than 100 characters")]
    string Title, 
    List<int> participantIds
);
public record ConversationResponse(int Id, string Title, List<int> ParticipantIds, List<string> ParticipantNames);
public record ConversationDetails(int Id, List<MessageResponse> Messages);
