using System.ComponentModel.DataAnnotations;

namespace Core.DTOs;

public record MessageData(
    [Required(ErrorMessage = "Message cannot be empty")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 2000 characters")]
    string Content,
    [Range(1, int.MaxValue, ErrorMessage = "ConversationId must be a valid integer")]
    int ConversationId
);

public record MessageResponse(
    string AuthorUsername,
    string Content,
    DateTime CreatedAt
);

public record EmailMessage(string To, string Subject, string Body);
