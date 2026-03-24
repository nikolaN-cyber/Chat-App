namespace Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AuthorId { get; set; }
        public User? Author { get; set; } = null;
        public int ConversationId { get; set; }
        public Conversation? Conversation { get; set; } = null;
    }
}
