using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Message: Comment
    {
        public string? Link { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MessageStatus Status { get; set; }
    }
}
