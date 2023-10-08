using backend.Models.Enums;

namespace backend.Models
{
    public class Post: Comment
    {
        public List<string> Likes { get; set; } = new();
        public Dictionary<string, Comment> Comments { get; set; } = new();
        public string? ImgUrl { get; set; }
        public Status? Status { get; set; }
        public string? GroupId { get; set; }
        public UserBase? Sender { get; set; }
        public UserBase? Recipient { get; set; }
        public Group? Group { get; set; }
    }
}
