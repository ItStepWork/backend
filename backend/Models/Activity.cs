using backend.Models.Enums;

namespace backend.Models
{
    public class Activity
    {
        public string? UserId { get; set; }
        public Page? Page { get; set; }
        public DateTime DateTime { get; set; }

    }
}
