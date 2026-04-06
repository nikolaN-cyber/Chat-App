using Domain.Entities;

namespace Core.DTOs.Message;

public record MessageResponse(
    string AuthorUsername,
    string Content,
    DateTime CreatedAt,
    string AuthorProfilePicture,
    string? FileUrl,
    string? FileType,
    int ConversationId,
    MessageType Type
);
