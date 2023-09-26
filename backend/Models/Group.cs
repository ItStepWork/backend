using backend.Models.Enums;

namespace backend.Models
{
    public class Group
    {
        public string? Id { get; set; }
        public string? PictureUrl { get; set; }
        public string? AdminId { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public Audience? Audience { get; set; }
        public string? Description { get; set; }
        public Status? Status { get; set; }
        public DateTime BlockingTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public Dictionary<string,bool> Users { get; set; }= new Dictionary<string,bool>();
    }
}
