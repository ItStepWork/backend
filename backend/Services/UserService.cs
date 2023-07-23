using backend.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace backend.Services
{
    public static class UserService
    {
        private static string firebaseDatabaseUrl = "https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/";
        private static readonly FirebaseClient firebaseClient = new FirebaseClient(firebaseDatabaseUrl);

        public static async Task<FirebaseObject<User>> AddUser(User user)
        {
            return await firebaseClient
              .Child("Users")
              .PostAsync(user);
        }
        public static async Task<List<KeyValuePair<string, User>>?> GetUsers()
        {
            var users = await firebaseClient
              .Child("Users")
              .OnceAsync<User>();

            return users?
              .Select(x => new KeyValuePair<string, User>(x.Key, x.Object))
              .ToList();
        }
        public static async Task UpdateUser(string userId, User user)
        {
            await firebaseClient
              .Child("Users")
              .Child(userId)
              .PutAsync(user);
        }
        public static async Task RemoveUser(string userId)
        {
            await firebaseClient
              .Child("Users")
              .Child(userId)
              .DeleteAsync();
        }
        public static async Task<FirebaseObject<User>?> FindUserByEmail(string email)
        {
            var users = await firebaseClient
              .Child("Users")
              .OnceAsync<User>();

            return users?
              .FirstOrDefault(user => user.Object.Email == email);
        }
        public static async Task<User?> FindUserById(string userId)
        {
            var user = await firebaseClient
              .Child("Users")
              .Child(userId).OnceSingleAsync<User>();

            return user;
        }
        public static async Task<Message?> SendMessage(string senderId, string recipientId, string text)
        {
            Message message = new Message();

            var resultTwo = await firebaseClient
             .Child($"Messages/{recipientId}/{senderId}")
             .PostAsync(message);

            var resultOne = await firebaseClient
             .Child($"Messages/{senderId}/{recipientId}")
             .PostAsync(message);

            if (resultOne?.Object != null && resultTwo?.Object != null) return message;
            else return null;
        }
    }
}
