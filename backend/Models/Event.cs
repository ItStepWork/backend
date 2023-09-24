using backend.Models.Enums;

namespace backend.Models
{
    public class Event
    {
        public UserBase? User { get; set; }
        public EventType? EventType { get; set; }
    }
}
