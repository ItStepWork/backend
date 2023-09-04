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
            FriendRequest recipient = new FriendRequest();
            recipient.UserId = senderId;
            recipient.SenderId = senderId;
            FriendRequest sender = new FriendRequest();
            sender.UserId = recipientId;
            sender.SenderId = senderId;

            await UpdateFriendAsync(recipientId, senderId, recipient);
            await UpdateFriendAsync(senderId, recipientId, sender);

            return true;
        }
        public static async Task<FriendRequest?> FindFriendAsync(string senderId, string recipientId)
        {
            return await firebaseDatabase
              .Child($"Friends/{senderId}")
              .Child(recipientId)
              .OnceSingleAsync<FriendRequest>();
        }
        private static async Task UpdateFriendAsync(string senderId, string recipientId, FriendRequest friend)
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
        public static async Task<IEnumerable<Friend?>?> GetFriendsAsync(string senderId, string userId)
        {
            if (senderId == userId)
            {
                var result = await firebaseDatabase
                  .Child($"Friends/{userId}")
                  .OnceAsync<FriendRequest>();

                var friends = result.Select(item => item.Object);
                var friensConfirmed = friends.Where(item => item.IsConfirmed == true).Select(item => item.UserId).ToArray();
                var friensUnconfirmed = friends.Where(item => item.IsConfirmed == false && item.SenderId != senderId).Select(item => item.UserId).ToArray();
                var friensWaiting = friends.Where(item => item.IsConfirmed == false && item.SenderId == senderId).Select(item => item.UserId).ToArray();
                List<Friend?> list = new();
                var users = await UserService.GetUsersAsync();
                var usersUnconfirmed = users?.Where(user=> friensUnconfirmed.Contains(user.Id)).Select(user=>user as Friend);
                if(usersUnconfirmed != null)
                {
                    foreach (var user in usersUnconfirmed)
                    {
                        if(user!= null) user.FriendStatus = FriendStatus.Unconfirmed;
                    }
                }
                var usersWaiting = users?.Where(user=> friensWaiting.Contains(user.Id)).Select(user => user as Friend);
                if (usersWaiting != null)
                {
                    foreach (var user in usersWaiting)
                    {
                        if (user != null) user.FriendStatus = FriendStatus.Waiting;
                    }
                }
                var userConfirmed = users?.Where(user=> friensConfirmed.Contains(user.Id)).Select(user => user as Friend);
                if (userConfirmed != null)
                {
                    foreach (var user in userConfirmed)
                    {
                        if (user != null) user.FriendStatus = FriendStatus.Confirmed;
                    }
                }
                var userOther = users?.Where(user => !friensUnconfirmed.Contains(user.Id) && !friensWaiting.Contains(user.Id) && !friensConfirmed.Contains(user.Id)).Select(user => user as Friend);
                if (userOther != null)
                {
                    foreach (var user in userOther)
                    {
                        if (user != null) user.FriendStatus = FriendStatus.Other;
                    }
                }
                if (usersUnconfirmed != null) list.AddRange(usersUnconfirmed);
                if (usersWaiting != null) list.AddRange(usersWaiting);
                if (userConfirmed != null) list.AddRange(userConfirmed);
                if (userOther != null) list.AddRange(userOther);
                return list;
            }
            else
            {
                var result = await firebaseDatabase
                  .Child($"Friends/{userId}")
                  .OnceAsync<FriendRequest>();

                var userFriends = result.Select(item => item.Object);
                var userFriensConfirmed = userFriends.Where(item => item.IsConfirmed == true).Select(item => item.UserId).ToArray();


                var resultFriends = await firebaseDatabase
                  .Child($"Friends/{senderId}")
                  .OnceAsync<FriendRequest>();
                var friends = resultFriends.Select(item => item.Object);
                var friensConfirmed = friends.Where(item => userFriensConfirmed.Contains(item.UserId) && item.IsConfirmed == true).Select(item => item.UserId).ToArray();
                var friensUnconfirmed = friends.Where(item => userFriensConfirmed.Contains(item.UserId) && item.IsConfirmed == false && item.SenderId != senderId).Select(item => item.UserId).ToArray();
                var friensWaiting = friends.Where(item => userFriensConfirmed.Contains(item.UserId) && item.IsConfirmed == false && item.SenderId == senderId).Select(item => item.UserId).ToArray();


                var allUsers = await UserService.GetUsersAsync();
                var users = allUsers?.Where(user => userFriensConfirmed.Contains(user.Id));
                List<Friend?> list = new();
                var usersUnconfirmed = users?.Where(user => friensUnconfirmed.Contains(user.Id)).Select(user => user as Friend);
                if (usersUnconfirmed != null)
                {
                    foreach (var user in usersUnconfirmed)
                    {
                        if (user != null) user.FriendStatus = FriendStatus.Unconfirmed;
                    }
                }
                var usersWaiting = users?.Where(user => friensWaiting.Contains(user.Id)).Select(user => user as Friend);
                if (usersWaiting != null)
                {
                    foreach (var user in usersWaiting)
                    {
                        if (user != null) user.FriendStatus = FriendStatus.Waiting;
                    }
                }
                var userConfirmed = users?.Where(user => friensConfirmed.Contains(user.Id)).Select(user => user as Friend);
                if (userConfirmed != null)
                {
                    foreach (var user in userConfirmed)
                    {
                        if (user != null) user.FriendStatus = FriendStatus.Confirmed;
                    }
                }
                var userOther = users?.Where(user => !friensUnconfirmed.Contains(user.Id) && !friensWaiting.Contains(user.Id) && !friensConfirmed.Contains(user.Id)).Select(user => user as Friend);
                if (userOther != null)
                {
                    foreach (var user in userOther)
                    {
                        if (user != null) user.FriendStatus = FriendStatus.Other;
                    }
                }
                if (usersUnconfirmed != null) list.AddRange(usersUnconfirmed);
                if (usersWaiting != null) list.AddRange(usersWaiting);
                if (userConfirmed != null) list.AddRange(userConfirmed);
                if (userOther != null) list.AddRange(userOther);
                return list;
            }
        }
        public static async Task<IEnumerable<UserBase>> GetConfirmedFriends(string senderId, string userId)
        {
            var friends = await firebaseDatabase
              .Child($"Friends/{userId}")
              .OnceAsync<FriendRequest>();

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
                      .OnceAsync<FriendRequest>();

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
              .OnceAsync<FriendRequest>();

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
                      .OnceAsync<FriendRequest>();

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
              .OnceAsync<FriendRequest>();

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
                      .OnceAsync<FriendRequest>();

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
              .OnceAsync<FriendRequest>();

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
                      .OnceAsync<FriendRequest>();

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
