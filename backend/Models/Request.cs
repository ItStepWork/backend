using backend.Models.Enums;

namespace backend.Models
{
    public class Request
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? RecipientId { get; set; }
        public string? SenderId { get; set; }
        public string? Email { get; set; }
        public string? GroupId { get; set; }
        public string? PhotoId { get; set; }
        public string? PostId { get; set; }
        public string? PhotoUrl { get; set; }
        public string? AlbumId { get; set; }
        public string? Text { get; set; }
        public string? Url { get; set; }
        public string? Name { get; set; }
        public IFormFile[]? Files { get; set; }
        public Audience? Audience { get; set; }
        public Status? Status { get; set; }
        public Role? Role { get; set; }
        public string? Description { get; set; }
        public IFormFile? File { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? BlockingTime { get; set; }
        public EventType? EventType { get; set; }
        public string? Date { get; set; }
        public Access? Access { get; set; }
    }
}
