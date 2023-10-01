using backend.Models.Enums;

namespace backend.Models
{
    public class Event
    {
        public UserBase? User { get; set; }
        public EventType? Type { get; set; }
        public DateTime? Date {  get; set; }
        public string? Id { get; set; }
        public string? Name { get; set;}
        public string? UserId { get; set;}
    }
}
