using System.Text.Json.Serialization;

namespace backend.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum NotificationType
    {
        AddFriend,
        RemoveFriend,
        ConfirmFriend,
        BirthDay,
        InviteToGroup,
        LikePhoto,
        CommentPhoto,
        CancelFriend,
        RefusedFriend,
    }
}
