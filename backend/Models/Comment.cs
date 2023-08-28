namespace backend.Models
{
    public class Comment
    {
        public string? Id { get; set; }
        public string? SenderId { get; set; }
        public string? RecipientId { get; set; }
        public string? Text { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
