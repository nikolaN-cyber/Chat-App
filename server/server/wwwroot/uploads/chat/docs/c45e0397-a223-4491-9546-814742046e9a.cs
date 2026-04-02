using Core.DTOs.Message;

namespace Core.DTOs.Conversation;

public record ConversationResponse(int Id, string Title, bool IsGroup, int UnreadCount, List<int> ParticipantIds, List<string> ParticipantNames, string? PhotoUrl);
public record ConversationDetails(int Id, List<MessageResponse> Messages, List<ParticipantNames> Participants, int AdminId);
public record ParticipantNames(string Username, int userId, string PhotoUrl);
