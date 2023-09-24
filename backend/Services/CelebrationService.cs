using backend.Models;
using backend.Models.Enums;
using Firebase.Database;
using Firebase.Storage;

namespace backend.Services
{
    public class CelebrationService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
        private static readonly FirebaseStorage firebaseStorage = new FirebaseStorage("database-50f39.appspot.com");
        public static async Task<IEnumerable<Event?>?> GetBirthdaysEventNowAsync(string userId)
        {
            var result = await firebaseDatabase
                .Child($"Friends/{userId}")
                .OnceAsync<FriendRequest>();
            if (result != null)
            {
                var users = result.Select(item => item.Object);
                var friendsConfirm = users.Where(u => u.IsConfirmed == true).Select(item=>item.UserId).ToArray();
                var allUsers = await UserService.GetUsersAsync(userId);
                if(allUsers != null) 
                {
                    var friends = allUsers.Where(item => friendsConfirm.Contains(item.Id));
                    var friendsBirthday = friends.Where(item => item.BirthDay.Month == DateTime.Now.Month && item.BirthDay.Day == DateTime.Now.Day).Select(item=> new Event() {User = item,EventType = EventType.BirthDay});
                    if(friendsBirthday != null) return friendsBirthday;
                }
            }
            return new Event[] { };
        }

        public static async Task<IEnumerable<Event?>?> GetBirthdaysEventSoonAsync(string userId)
        {
            var result = await firebaseDatabase
                .Child($"Friends/{userId}")
                .OnceAsync<FriendRequest>();
            if (result != null)
            {
                var users = result.Select(item => item.Object);
                var friendsConfirm = users.Where(u => u.IsConfirmed == true).Select(item => item.UserId).ToArray();
                var allUsers = await UserService.GetUsersAsync(userId);
                if (allUsers != null)
                {
                    var friends = allUsers.Where(item => friendsConfirm.Contains(item.Id));
                    var friendsBirthday = friends.Where(item => (item.BirthDay.Month == DateTime.Now.Month && item.BirthDay.Day > DateTime.Now.Day) || item.BirthDay.Month == DateTime.Now.Month + 1).Select(item => new Event() { User = item, EventType = EventType.BirthDay });
                    if (friendsBirthday != null) return friendsBirthday;
                }
            }
            return new Event[] { };
        }
    }
}
