using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Friend: UserBase
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FriendStatus FriendStatus { get; set; }
    }
}
