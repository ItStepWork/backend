namespace backend.Models
{
    public class User
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; }
        public Status Status { get; set; }
        public DateTime LastVisit { get; set; }
    }
}
