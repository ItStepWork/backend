namespace backend.Models
{
    public class UserBase
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public Role Role { get; set; }
        public Status Status { get; set; }
        public DateTime LastVisit { get; set; }
    }
}
