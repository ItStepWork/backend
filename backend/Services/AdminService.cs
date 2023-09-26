using backend.Models;
using backend.Models.Enums;
using Firebase.Database;
using Firebase.Database.Query;
using System.Security.Claims;

namespace backend.Services
{
    public static class AdminService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");

        public static async Task<(string response, User? user)> ValidationAdmin(HttpContext httpContext)
        {
            Claim? claimRole = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            Claim? claimId = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid);
            if (claimId == null || claimRole == null) return ("User not authorize!", null);

            User? sender = await UserService.FindUserByIdAsync(claimId.Value);
            if (sender == null) return ("Sender not found!", null);

            if (sender.Role != Models.Enums.Role.Admin || sender.Role.ToString() != claimRole.Value) return ("User not access!", null);

            await UserService.UpdateUserLastVisitAsync(claimId.Value);
            return ("", sender);
        }
        public static async Task<ChartActivity> GetPagesActivityAsync(Chart chart)
        {
            var result = await ActivityService.GetAllActivityAsync();
            var resultContacts = result?.Where(x => x.Page == Models.Enums.Page.Contacts).GroupBy(x => $"{x.DateTime.Year}-{x.DateTime.Month}-{x.DateTime.Day}" + ( chart == Chart.Hourly ? $"-{x.DateTime.Hour}" : ""));
            var resultGallery = result?.Where(x => x.Page == Models.Enums.Page.Gallery).GroupBy(x => $"{x.DateTime.Year}-{x.DateTime.Month}-{x.DateTime.Day}" + (chart == Chart.Hourly ? $"-{x.DateTime.Hour}" : ""));
            var resultGroups = result?.Where(x => x.Page == Models.Enums.Page.Groups).GroupBy(x => $"{x.DateTime.Year}-{x.DateTime.Month}-{x.DateTime.Day}" + (chart == Chart.Hourly ? $"-{x.DateTime.Hour}" : ""));
            var resultNotifications = result?.Where(x => x.Page == Models.Enums.Page.Notifications).GroupBy(x => $"{x.DateTime.Year}-{x.DateTime.Month}-{x.DateTime.Day}" + (chart == Chart.Hourly ? $"-{x.DateTime.Hour}" : ""));
            var resultMessaging = result?.Where(x => x.Page == Models.Enums.Page.Messaging).GroupBy(x => $"{x.DateTime.Year}-{x.DateTime.Month}-{x.DateTime.Day}" + (chart == Chart.Hourly ? $"-{x.DateTime.Hour}" : ""));
            
            ChartActivity chartActivity = new();
            chartActivity.Contacts = resultContacts?.Select(item => new Point() { Y = item.Count(), X = item.ElementAt(0).DateTime });
            chartActivity.Gallery = resultGallery?.Select(item => new Point() { Y = item.Count(), X = item.ElementAt(0).DateTime });
            chartActivity.Groups = resultGroups?.Select(item => new Point() { Y = item.Count(), X = item.ElementAt(0).DateTime });
            chartActivity.Notifications = resultNotifications?.Select(item => new Point() { Y = item.Count(), X = item.ElementAt(0).DateTime });
            chartActivity.Messaging = resultMessaging?.Select(item => new Point() { Y = item.Count(), X = item.ElementAt(0).DateTime });
            return chartActivity;
        }
        public static async Task<IEnumerable<Point>?> GetUsersActivityAsync(Chart chart)
        {
            var result = await ActivityService.GetAllActivityAsync();
            var newResult = result?.Select(x => { 
                if(chart == Chart.Hourly) x.DateTime = new DateTime(x.DateTime.Year, x.DateTime.Month, x.DateTime.Day, x.DateTime.Hour, 30, 0, DateTimeKind.Utc);
                else x.DateTime = new DateTime(x.DateTime.Year, x.DateTime.Month, x.DateTime.Day, 12, 0, 0, DateTimeKind.Utc);
                return x; 
            });
            var group = newResult?.GroupBy(x => x.DateTime);
            var points = group?.Select(list => new Point() { Y = list.GroupBy(y => y.UserId).Count(), X = list.ElementAt(0).DateTime });
            return points;
        }
        public static async Task UpdateGroupStatusAsync(string groupId, Status status)
        {
            await firebaseDatabase
              .Child("Groups")
              .Child(groupId)
              .Child("Status")
              .PutAsync<int>((int)status);
        }
        public static async Task UpdateGroupBlockingTimeAsync(string groupId, DateTime dateTime)
        {
            await firebaseDatabase
              .Child("Groups")
              .Child(groupId)
              .Child("BlockingTime")
              .PutAsync<string>(dateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK"));
        }
    }
}
