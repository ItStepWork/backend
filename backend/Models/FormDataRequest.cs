namespace backend.Models
{
    public class FormDataRequest
    {
        public string? Email {get; set;}
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public Gender Gender { get; set; }
        public string? FamilyStatus { get; set; }
        public string? Born { get; set; }
        public string? AboutMe { get; set; }
    }
}
