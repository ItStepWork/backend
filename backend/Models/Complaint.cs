namespace backend.Models
{
    public class Complaint: Message
    {
        public string? UserId { get; set; }
        public string? PhotoId { get; set; }
        public string? PhotoUrl { get; set; }
        public UserBase? Sender { get; set; }
        public UserBase? User { get; set; }
    }
}
