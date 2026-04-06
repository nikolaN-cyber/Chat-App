using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Message;

public record MessageData(
    string? Content,
    [Range(1, int.MaxValue, ErrorMessage = "ConversationId must be a valid integer")]
    int ConversationId,
    string? FileUrl,
    string? FileType
);

public record EmailMessage(string To, string Subject, string Body);