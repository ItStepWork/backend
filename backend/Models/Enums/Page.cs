using System.Text.Json.Serialization;

namespace backend.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Page
    {
        Contacts,
        Messaging,
        Gallery,
        Notifications,
        Groups,
        Authorization,
    }
}
