using backend.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace backend.Services
{
    public static class UserService
    {
        private static string firebaseDatabaseUrl = "https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/";
        private static readonly FirebaseClient firebaseClient = new FirebaseClient(firebaseDatabaseUrl);

        public static async Task<FirebaseObject<User>> Add(User user)
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
        public static async Task UpdateUser(string id, User user)
        {
            await firebaseClient
              .Child("Users")
              .Child(id)
              .PutAsync(user);
        }
        public static async Task RemoveUser(string id)
        {
            await firebaseClient
              .Child("students")
              .Child(id)
              .DeleteAsync();
        }
        public static async Task<FirebaseObject<User>?> FindUser(string email)
        {
            var users = await firebaseClient
              .Child("Users")
              .OnceAsync<User>();

            return users?
              .FirstOrDefault(user => user.Object.Email == email);
        }
    }
}
