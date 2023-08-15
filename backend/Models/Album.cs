namespace backend.Models
{
    public class Album
    {
        public string Id { get; set; } = String.Empty;
        public string Name { get; set; }
        public Dictionary<string, Photo> Photos { get; set; } = new Dictionary<string, Photo>();
    }
}
