using System.Xml.Linq;

namespace backend.Models
{
    public class Response
    {
        public string? AccessToken { get; set; }
        public UserBase User { get; set; }
        public Response(UserBase userBase, string? accessToken)
        {
            AccessToken = accessToken;
            User = userBase;
        }
    }
}
