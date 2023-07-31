using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Message
    {
        public string? Id { get; set; }
        public string? SenderId { get; set; }
        public string? Text { get; set; }
        public DateTime CreateTime { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MessageStatus Status { get; set; }
    }
}
