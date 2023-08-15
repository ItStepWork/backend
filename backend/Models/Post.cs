namespace backend.Models
{
    public class Post
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? Text { get; set; }
        public string? MediaUrl { get; set; }
        public int LikesCount { get; set; }
        public int RepostsCount { get; set; }
        public List<Comment>? Comments { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
