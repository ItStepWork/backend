using System.Text.Json.Serialization;
using backend.Models.Enums;

namespace backend.Models
{
    public class UserBase
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public Role? Role { get; set; }
        public Status? Status { get; set; }
        public DateTime BirthDay { get; set; }
        public DateTime LastVisit { get; set; }
        public DateTime CreatedTime { get; set; }
        public string? Born { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? FamilyStatus { get; set; }
        public Gender? Gender { get; set; }
        public string? AboutMe { get; set; } 
        public string? Location { get; set; }
        public string? Work { get; set; }
        public string? Joined { get; set; }
        public string? AvatarUrl { get; set; }
        public string? BackgroundUrl { get; set; }
        public string? IpAddress { get; set; }
    }
}
