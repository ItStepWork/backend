namespace backend.Models
{
    public class PostRequest
    {
        public string? UserId { get; set; }
        public string? PostId { get; set; }
        public string? Text { get; set; }
        public string? Url { get; set; }
    }
}
