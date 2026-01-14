using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class SupportChatMessage
    {
        public long Id { get; set; }

        public Guid SessionId { get; set; }
        public SupportChatSession Session { get; set; } = default!;

        public string UserMessage { get; set; } = "";
        public string BotReply { get; set; } = "";

        public string? Model { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
