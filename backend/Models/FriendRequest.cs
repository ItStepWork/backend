namespace backend.Models
{
    public class FriendRequest
    {
        public string? SenderId { get; set; }
        public string? UserId { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
