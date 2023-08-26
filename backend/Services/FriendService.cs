using backend.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace backend.Services
{
    public class FriendService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
        public static async Task<bool> AddFriendAsync(string senderId, string recipientId)
        {
            Friend recipient = new Friend();
            recipient.UserId = senderId;
            recipient.SenderId = senderId;
            Friend sender = new Friend();
            sender.UserId = recipientId;
            sender.SenderId = senderId;

            await UpdateFriendAsync(recipientId, senderId, recipient);
            await UpdateFriendAsync(senderId, recipientId, sender);

            return true;
        }
        public static async Task<Friend?> FindFriendAsync(string senderId, string recipientId)
        {
            return await firebaseDatabase
              .Child($"Friends/{senderId}")
              .Child(recipientId)
              .OnceSingleAsync<Friend>();
        }
        private static async Task UpdateFriendAsync(string senderId, string recipientId, Friend friend)
        {
            await firebaseDatabase
              .Child("Friends")
              .Child(senderId)
              .Child(recipientId)
              .PutAsync(friend);
        }
        public static async Task RemoveFriendAsync(string senderId, string recipientId)
        {
            await firebaseDatabase
              .Child("Friends")
              .Child(senderId)
              .Child(recipientId)
              .DeleteAsync();
            await firebaseDatabase
              .Child("Friends")
              .Child(recipientId)
              .Child(senderId)
              .DeleteAsync();
        }
        public static async Task<bool> ConfirmFriendAsync(string senderId, string recipientId)
        {
            var recipient = await FindFriendAsync(senderId, recipientId);
            var sender = await FindFriendAsync(recipientId, senderId);

            if (recipient != null && sender != null)
            {
                recipient.IsConfirmed = true;
                sender.IsConfirmed = true;
                await UpdateFriendAsync(senderId, recipientId, recipient);
                await UpdateFriendAsync(recipientId, senderId, sender);
                return true;
            }
            else return false;
        }
        public static async Task<IEnumerable<Friend>?> GetFriendsAsync(string id)
        {
            var friends = await firebaseDatabase
              .Child($"Friends/{id}")
              .OnceAsync<Friend>();

            return friends?
              .Select(x => x.Object);
        }
        public static async Task<IEnumerable<UserBase>> GetConfirmedFriends(string senderId, string userId)
        {
            var friends = await firebaseDatabase
              .Child($"Friends/{userId}")
              .OnceAsync<Friend>();

            var users = await UserService.GetUsersAsync();

            if (friends != null && friends.Count > 0 && users != null)
            {
                if (senderId == userId)
                {
                    string?[] filter = friends.Where(x => x.Object.IsConfirmed == true).Select(x => x.Object.UserId).ToArray();
                    var result = users.Where(user => filter.Contains(user.Id) && user.Id != senderId);
                    return result;
                }
                else
                {
                    var friendsSender = await firebaseDatabase
                      .Child($"Friends/{senderId}")
                      .OnceAsync<Friend>();

                    if (friendsSender != null && friendsSender.Count > 0)
                    {
                        string?[] filter = friends.Select(x => x.Object.UserId).ToArray();
                        string?[] filterSender = friendsSender.Where(x => x.Object.IsConfirmed == true).Select(x => x.Object.UserId).ToArray();
                        var result = users.Where(user => filter.Contains(user.Id) && filterSender.Contains(user.Id));
                        return result;
                    }
                    else return new UserBase[] { };
                }
            }
            return new UserBase[] { };
        }
        public static async Task<IEnumerable<UserBase>> GetUnconfirmedFriends(string senderId, string userId)
        {
            var friends = await firebaseDatabase
              .Child($"Friends/{userId}")
              .OnceAsync<Friend>();

            var users = await UserService.GetUsersAsync();

            if (friends != null && friends.Count > 0 && users != null)
            {
                if (senderId == userId)
                {
                    string?[] filter = friends.Where(x => x.Object.IsConfirmed == false && x.Object.SenderId != senderId).Select(x => x.Object.UserId).ToArray();
                    var result = users.Where(user => filter.Contains(user.Id) && user.Id != senderId);
                    return result;
                }
                else
                {
                    var friendsSender = await firebaseDatabase
                      .Child($"Friends/{senderId}")
                      .OnceAsync<Friend>();

                    if (friendsSender != null && friendsSender.Count > 0)
                    {
                        string?[] filter = friends.Select(x => x.Object.UserId).ToArray();
                        string?[] filterSender = friendsSender.Where(x => x.Object.IsConfirmed == false && x.Object.SenderId != senderId).Select(x => x.Object.UserId).ToArray();
                        var result = users.Where(user => filter.Contains(user.Id) && filterSender.Contains(user.Id));
                        return result;
                    }
                    else return new UserBase[] { };
                }
            }
            return new UserBase[] { };
        }
        public static async Task<IEnumerable<UserBase>> GetWaitingFriends(string senderId, string userId)
        {
            var friends = await firebaseDatabase
              .Child($"Friends/{userId}")
              .OnceAsync<Friend>();

            var users = await UserService.GetUsersAsync();

            if (friends != null && friends.Count > 0 && users != null)
            {
                if (senderId == userId)
                {
                    string?[] filter = friends.Where(x => x.Object.IsConfirmed == false && x.Object.SenderId == senderId).Select(x => x.Object.UserId).ToArray();
                    var result = users.Where(user => filter.Contains(user.Id) && user.Id != senderId);
                    return result;
                }
                else
                {
                    var friendsSender = await firebaseDatabase
                      .Child($"Friends/{senderId}")
                      .OnceAsync<Friend>();

                    if (friendsSender != null && friendsSender.Count > 0)
                    {
                        string?[] filter = friends.Select(x => x.Object.UserId).ToArray();
                        string?[] filterSender = friendsSender.Where(x => x.Object.IsConfirmed == false && x.Object.SenderId == senderId).Select(x => x.Object.UserId).ToArray();
                        var result = users.Where(user => filter.Contains(user.Id) && filterSender.Contains(user.Id));
                        return result;
                    }
                    else return new UserBase[] { };
                }
            }
            return new UserBase[] { };
        }
        public static async Task<IEnumerable<UserBase>> GetOtherUsers(string senderId, string userId)
        {
            var friends = await firebaseDatabase
              .Child($"Friends/{userId}")
              .OnceAsync<Friend>();

            var users = await UserService.GetUsersAsync();

            if (friends != null && friends.Count > 0 && users != null)
            {
                if(senderId == userId)
                {
                    string?[] filter = friends.Select(x => x.Object.UserId).ToArray();
                    var result = users.Where(user => !filter.Contains(user.Id) && user.Id != senderId);
                    return result;
                }
                else
                {
                    var friendsSender = await firebaseDatabase
                      .Child($"Friends/{senderId}")
                      .OnceAsync<Friend>();

                    string?[] filter = friends.Where(x => x.Object.IsConfirmed == true).Select(x => x.Object.UserId).ToArray();
                    string?[] filterSender = friendsSender.Select(x => x.Object.UserId).ToArray();
                    if(filterSender.Length > 0)
                    {
                        var result = users.Where(user => filter.Contains(user.Id) && !filterSender.Contains(user.Id));
                        return result;
                    }
                    else
                    {
                        var result = users.Where(user => filter.Contains(user.Id));
                        return result;
                    }
                }
            }
            else if(users != null && senderId == userId) return users;
            else return new UserBase[] { };
        }
    }
}
