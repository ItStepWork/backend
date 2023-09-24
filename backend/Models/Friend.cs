using backend.Models.Enums;

namespace backend.Models
{
    public class Friend: UserBase
    {
        public FriendStatus? FriendStatus { get; set; }
    }
}
