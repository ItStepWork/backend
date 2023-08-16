namespace backend.Models
{
    public class Album
    {
        public string Id { get; set; } = String.Empty;
        public string Name { get; set; }
        public List<string> Photos { get; set; } = new();
    }
}
