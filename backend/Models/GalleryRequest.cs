namespace backend.Models
{
    public class GalleryRequest
    {
        public string? UserId { get; set; }
        public string? PhotoId { get; set; }
        public string? Text { get; set; }
        public string? Url { get; set; }
        public string? Name { get; set; }
        public IFormFile[]? Files { get; set; }
    }
}
