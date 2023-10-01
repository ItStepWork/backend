using System.Text.Json.Serialization;

namespace backend.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EventType
    {
        Meeting,
        Celebration,
        BirthDay,
    }
}
