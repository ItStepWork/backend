namespace backend.Models
{
    public class ChartActivity
    {
        public IEnumerable<Point>? Contacts { get; set; }
        public IEnumerable<Point>? Messaging { get; set; }
        public IEnumerable<Point>? Gallery { get; set; }
        public IEnumerable<Point>? Notifications { get; set; }
        public IEnumerable<Point>? Groups { get; set; }
    }
}
