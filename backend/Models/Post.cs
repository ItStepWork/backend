namespace backend.Models
{
    public class Post
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? Text { get; set; }
        public int Likes { get; set; }
        public int Reposts { get; set; }
        public List<Comment>? Comments { get; set; }
        public DateTime CreateTime { get; set; }
        public string? ImageUrl { get; internal set; }
        public string? VideoUrl { get; internal set; }
    }
}
