namespace backend.Models
{
    public class Complaint: Message
    {
        public string? UserId { get; set; }
        public string? PhotoId { get; set; }
    }
}
