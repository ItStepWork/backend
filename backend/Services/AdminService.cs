using backend.Models;
using backend.Models.Enums;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Services
{
    public static class AdminService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");

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
        public static async Task SendSupportMessageAsync(string senderId, Request request)
        {
            Message message = new Message();
            message.Id = Guid.NewGuid().ToString("N");
            message.Text = request.Text;
            message.CreateTime = DateTime.UtcNow;
            message.SenderId = senderId;
            message.Status = MessageStatus.Unread;

            if (request.File != null)
            {
                string? link = await UserService.SaveFileAsync(request.File, "Support", message.Id);
                message.Link = link;
            }

            await firebaseDatabase
             .Child($"Support/Messages/{request.UserId}/{message.Id}")
             .PutAsync(message);
        }
        public static async Task<IEnumerable<Dialog>?> GetSupportDialogsAsync()
        {
            var dialogs = await firebaseDatabase.Child($"Support/Messages")
                .OnceAsync<IDictionary<string, Message>>();

            var users = await UserService.GetUsersAsync();
            var result = dialogs.Select(x => new Dialog() { User = users?.FirstOrDefault(u => u.Id == x.Key), LastMessage = x.Object.OrderBy(m=>m.Value.CreateTime).LastOrDefault().Value });
            return result;
        }
        public static async Task<IEnumerable<Complaint>?> GetComplaintsAsync()
        {
            var result = await firebaseDatabase.Child($"Complaints")
                .OnceAsync<Complaint>();

            var users = await UserService.GetUsersAsync();
            var groups = await GroupService.GetGroupsAsync();
            var posts = await PostService.GetPostsAsync();

            return result?.Select(x => x.Object).OrderByDescending(m => m.CreateTime).Select(c => { 
                c.Sender = users?.FirstOrDefault(u => u.Id == c.SenderId);
                c.User = users?.FirstOrDefault(u => u.Id == c.UserId);
                c.Group = groups?.FirstOrDefault(u => u.Id == c.GroupId);
                c.Post = posts?.FirstOrDefault(p => p.Id == c.PostId);
                return c;
            });
        }
        public static async Task UpdateComplaintStatusAsync(string id)
        {
            await firebaseDatabase
              .Child("Complaints")
              .Child(id)
              .Child("Status")
              .PutAsync<int>((int)MessageStatus.Read);
        }
        public static async Task<IEnumerable<Group>?> GetGroupsAsync()
        {
            var groups = await firebaseDatabase
              .Child("Groups")
              .OnceAsync<Group>();
            return groups?.Select(x => x.Object);
        }
    }
}
