namespace backend.Models
{
    public class Photo
    {
        public string Id { get; set; } = String.Empty;
        public string AlbumId { get; set; } = String.Empty;
        public string StoryId { get; set; } = String.Empty;
        public string Url { get; set; }
        public List<string> Likes { get; set; } = new();
        public Dictionary<string, Comment> Comments { get; set; } = new();
    }
}
