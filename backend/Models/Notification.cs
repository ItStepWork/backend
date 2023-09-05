using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Notification
    {
        public string? Id { get; set; }
        public string? SenderId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NotificationType? Type { get; set; }
    }
}
