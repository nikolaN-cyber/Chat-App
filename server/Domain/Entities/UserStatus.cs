namespace Domain.Entities
{
    public class UserStatus
    {
        public int Id { get; set; }
        public string Emoji { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }

        public User? User { get; set; } = null;
        public int UserId { get; set; }
    }
}
