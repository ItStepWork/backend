using backend.Models.Enums;

namespace backend.Models
{
    public class Message: Comment
    {
        public string? Link { get; set; }
        public MessageStatus? Status { get; set; }
    }
}
