using Firebase.Database;

namespace backend.Services
{
    public class NotificationService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
        public static async Task AddNotificationAsync(string senderId, string recipientId)
        {

        }
    }
}
