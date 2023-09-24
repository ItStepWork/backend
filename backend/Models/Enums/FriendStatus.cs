using System.Text.Json.Serialization;

namespace backend.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FriendStatus
    {
        Confirmed,
        Unconfirmed,
        Waiting,
        Other,
    }
}
