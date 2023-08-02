using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Group
    {
        public string? Id { get; set; }
        public string? AdminId { get; set; }
        public string? Name { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Audience Audience { get; set; }
        public string? Description { get; set; }
        public Dictionary<string,bool> Users { get; set; }= new Dictionary<string,bool>();
    }
}
