using System.Text.Json.Serialization;

namespace backend.Models
{
    public class UserBase
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Role Role { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Status Status { get; set; }
        public DateTime LastVisit { get; set; }
        public string? Born { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? FamilyStatus { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Gender Gender { get; set; }
        public string? AboutMe { get; set; } 
        public string? Location { get; set; }
        public string? Work { get; set; }
        public string? Joined { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
