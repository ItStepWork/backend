using System.Text.Json.Serialization;
using backend.Models.Enums;

namespace backend.Models
{
    public class Notification
    {
        public string? Id { get; set; }
        public string? SenderId { get; set; }
        public string? Url { get; set; }
        public string? GroupName { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NotificationType? Type { get; set; }
        public DateTime DateTime { get; set; }

    }
}
