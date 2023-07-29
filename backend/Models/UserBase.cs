namespace backend.Models
{
    public class UserBase
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public Role Role { get; set; }
        public Status Status { get; set; }
        public DateTime LastVisit { get; set; }
        public string? Born { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? FamilyStatus { get; set; }
        public Gender Gender { get; set; }
        public string? AboutMe { get; set; } 
    }
}
