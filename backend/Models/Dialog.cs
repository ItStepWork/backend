namespace backend.Models
{
    public class Dialog
    {
        public UserBase? User { get; set; }
        public Message? LastMessage { get; set; }
        public int UnreadMessages { get; set; }
    }
}
