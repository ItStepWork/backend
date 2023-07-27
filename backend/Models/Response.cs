using System.Xml.Linq;

namespace backend.Models
{
    public class Response : UserBase
    {
        public string? AccessToken { get; set; }
        public Response(UserBase userBase, string? accessToken)
        {
            AccessToken = accessToken;
            Id = userBase.Id;
            Email = userBase.Email;
            Role = userBase.Role;
            Status = userBase.Status;
            LastVisit = userBase.LastVisit;
        }
    }
}
