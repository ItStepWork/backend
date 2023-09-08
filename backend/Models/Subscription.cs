using System.Net.WebSockets;

namespace backend.Models
{
    public class Subscription
    {
        public WebSocket WebSocket { get; set; }
        public DateTime EndTime { get; set; }
    }
}
