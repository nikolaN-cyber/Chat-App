namespace Domain.Entities
{
    public class Conversation
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public bool IsGroup { get; set; }
        public int LastMessageAddedId { get; set; }
        public int UnreadCount { get; set; }
        public List<Participation> Participants { get; set; } = new();
        public List<Message> Messages { get; set; } = new();
        public int AdminId { get; set; }
        public User? Admin { get; set; } = null;
    }
}
