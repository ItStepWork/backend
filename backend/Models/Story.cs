namespace backend.Models
{
    public class Story
    {
        public string? Id { get; set; } = String.Empty;
        public string? Name { get; set; } = String.Empty;
        public DateTime CreatedTime { get; set; }
        public IEnumerable<Photo>? Photos { get; set; }
    }
}
