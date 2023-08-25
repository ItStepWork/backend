namespace backend.Models
{
    public class Friend
    {
        public string? SenderId { get; set; }
        public string? UserId { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
