namespace backend.Models
{
    public class Post
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? Text { get; set; }
        public List<string> Likes { get; set; } = new();
        public Dictionary<string, Comment> Comments { get; set; } = new();
        public DateTime CreateTime { get; set; }
        public string? ImageUrl { get; internal set; }
    }
}
