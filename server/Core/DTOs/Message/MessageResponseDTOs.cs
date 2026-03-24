namespace Core.DTOs.Message;

public record MessageResponse(
    string AuthorUsername,
    string Content,
    DateTime CreatedAt
);
