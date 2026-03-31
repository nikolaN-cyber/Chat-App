using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class EventAttendance
    {
        public int OrganizerId { get; set; }
        public User Organizer { get; set; } = null;


        public int EventId { get; set; }
        public Event Event { get; set; } = null;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}
