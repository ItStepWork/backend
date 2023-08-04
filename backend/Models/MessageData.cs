namespace backend.Models
{
    public class MessageData
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public IFormFile? File { get; set; }

    }
}
