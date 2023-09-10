using System.Text.Json.Serialization;
using backend.Models.Enums;

namespace backend.Models
{
    public class Friend: UserBase
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FriendStatus? FriendStatus { get; set; }
    }
}
