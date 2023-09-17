using backend.Models.Enums;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Activity
    {
        public string? UserId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Page? Page { get; set; }
        public DateTime DateTime { get; set; }

    }
}
