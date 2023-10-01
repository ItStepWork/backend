namespace backend.Models
{
    public static class General
    {
        public static string SiteUrl = "http://localhost:3000/ua/";
        public static string GetBlockingTime(DateTime dateTime)
        {
            TimeSpan timeSpan = dateTime - DateTime.UtcNow;
            if (timeSpan < TimeSpan.FromDays(1)) return $"Blocked for {(dateTime - DateTime.UtcNow).ToString(@"hh\:mm\:ss")}";
            else return $"Blocked for {(dateTime - DateTime.UtcNow).ToString(@"dd")} days";
        }
    }
}
