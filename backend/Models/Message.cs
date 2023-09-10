using System.Text.Json.Serialization;
using backend.Models.Enums;

namespace backend.Models
{
    public class Message: Comment
    {
        public string? Link { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MessageStatus? Status { get; set; }
    }
}
