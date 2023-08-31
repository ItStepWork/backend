using System.Text.Json.Serialization;

namespace backend.Models
{
    public class GroupRequest
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? UserId { get; set; }
        public string? PhotoId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Audience? Audience { get; set; }
        public string? Description { get; set; }
        public IFormFile? File { get; set; }
    }
}
