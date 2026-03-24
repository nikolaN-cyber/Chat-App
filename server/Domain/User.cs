namespace Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public DateTime RegisteredAt { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime LastActive { get; set; } = DateTime.UtcNow;
        public string? PhotoUrl { get; set; }
        public List<Participation> Participations { get; set; } = new();
        public List<Conversation> AdminAt { get; set; } = new();
    }
}
