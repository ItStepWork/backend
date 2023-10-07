using backend.Models;
using backend.Models.Enums;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using MimeKit;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace backend.Services
{
    public class CelebrationService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
        private static readonly FirebaseStorage firebaseStorage = new FirebaseStorage("database-50f39.appspot.com");

        public static async Task AddEventAsync(Request request, string userId)
        {
            Event newEvent = new Event() {
                Date = DateTime.Parse(request.Date).ToUniversalTime(),
                Name = request.Name,
                UserId = userId, 
                Type = request.EventType,
                Id = Guid.NewGuid().ToString("N") 
            };
            await firebaseDatabase
              .Child("Events")
              .Child(userId)
              .Child(newEvent.Id)
              .PutAsync(newEvent);
            //if (result == null) return ("Event not added", "");
            //newEvent.Id = result.Key;
            //await UpdateEventAsync(newEvent, userId);
        }
        public static async Task UpdateEventAsync(Event _event)
        {
            await firebaseDatabase
              .Child("Events")
              .Child(_event.UserId)
              .Child(_event.Id)
              .PutAsync(_event);
        }

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
                    var friendsBirthday = friends.Where(item => item.BirthDay.Month == DateTime.Now.Month && item.BirthDay.Day == DateTime.Now.Day).Select(item=> new Event() {User = item,Type = EventType.BirthDay});
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
                    var friendsBirthday = friends.Where(item => (item.BirthDay.Month == DateTime.Now.Month && item.BirthDay.Day > DateTime.Now.Day) || item.BirthDay.Month == DateTime.Now.Month + 1)
                        .Select(item => new Event() 
                        { 
                            User = item,
                            Type = EventType.BirthDay,
                            Date = GetDate(item.BirthDay),
                            
                            
                        });
                    if (friendsBirthday != null) return friendsBirthday;
                }
            }
            return new Event[] { };
        }

        public static async Task<IEnumerable<Event?>?> GetAllEventsAsync()
        {
            var events = await firebaseDatabase
             .Child("Events")
             .OnceAsync<Dictionary<string,Event>>();
            var result = events.Select(e => e.Object).SelectMany(e => e.Values.ToList());
            return result;
        }

        public static async Task<IEnumerable<Event?>?> GetEventsAsync(string userId)
        {
            var events = await GetAllEventsAsync();

            var result = await firebaseDatabase
                .Child($"Friends/{userId}")
                .OnceAsync<FriendRequest>();
            var friends = await FriendService.GetConfirmedFriends(userId, userId);
            var friendIds = friends.Select(x => x.Id);
            var myFriendsEvents = events.Where(item => friendIds.Contains(item.UserId)).Where(i=>i.Date >= DateTime.Now.Date).OrderBy(x => x.Date);
            var final = myFriendsEvents.Select(x => { x.User = friends.FirstOrDefault(i => i.Id == x.UserId); return x; });
            return final;
        }

        public static DateTime GetDate(DateTime date)
        {
            var result = date;
            result = result.AddYears(DateTime.Now.Year - date.Year);
            return result;
        }
    }
}
