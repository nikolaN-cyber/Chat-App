using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class Conversation
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsGroup { get; set; }
        public List<Participation> Participants { get; set; } = new();
        public List<Message> Messages { get; set; } = new();
    }
}
