namespace Domain
{
    public class Review
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; } = null;

        public int EventId { get; set; }
        public Event? Event { get; set; } = null;

        public string Comment { get; set; }
        public int Rating { get; set; }
    }
}
