using backend.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace backend.Services
{
    public class NotificationService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
        public static async Task AddNotificationAsync(string senderId, string recipientId, NotificationType type)
        {
            Notification notification = new();
            notification.SenderId = senderId;
            notification.Type = type;
            notification.Id = Guid.NewGuid().ToString("N");
            notification.DateTime = DateTime.UtcNow;

            await firebaseDatabase.Child("Notifications").Child(recipientId).Child(notification.Id).PutAsync(notification);
        }
        public static async Task AddNotificationAsync(string senderId, string recipientId, string groupId, NotificationType type)
        {
            Notification notification = new();
            notification.SenderId = senderId;
            notification.Type = type;
            notification.Id = Guid.NewGuid().ToString("N");
            notification.DateTime = DateTime.UtcNow;
            notification.Url = $"{General.SiteUrl}group/{groupId}";

            await firebaseDatabase.Child("Notifications").Child(recipientId).Child(notification.Id).PutAsync(notification);
        }
        public static async Task<IEnumerable<NotificationResponse>?> GetNotificationsAsync(string userId)
        {
            var notifications = await firebaseDatabase
                .Child("Notifications")
                .Child(userId)
                .OnceAsync<Notification>();

            var users = await UserService.GetUsersAsync();

            return notifications?.Select(x => new NotificationResponse() { Notification = x.Object, User = users?.FirstOrDefault(user=>user.Id == x.Object.SenderId) } );
        }
    }
}
