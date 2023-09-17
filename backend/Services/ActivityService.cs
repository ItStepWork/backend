using backend.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace backend.Services
{
    public class ActivityService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");

        public static async Task<FirebaseObject<Activity>> AddActivityAsync(Activity activity)
        {
            var result = await firebaseDatabase
              .Child("Activity")
              .PostAsync(activity);
            return result;
        }
        public static async Task<IEnumerable<Activity>?> GetAllActivityAsync()
        {
            var result = await firebaseDatabase
              .Child("Activity")
              .OnceAsync<Activity>();
            return result?.Select(x=>x.Object);
        }
    }
}
