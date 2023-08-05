using backend.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using Newtonsoft.Json;
using System.IO;

namespace backend.Services
{
    public static class UserService
    {
        private static string firebaseDatabaseUrl = "https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/";
        private static readonly FirebaseClient firebaseClient = new FirebaseClient(firebaseDatabaseUrl);
        private static readonly FirebaseStorage firebaseStorage = new FirebaseStorage("database-50f39.appspot.com");

        public static async Task<FirebaseObject<User>> AddUserAsync(User user)
        {
            return await firebaseClient
              .Child("Users")
              .PostAsync(user);
        }
        public static async Task<IEnumerable<UserBase>?> GetUsersAsync(string userId)
        {
            var users = await firebaseClient
              .Child("Users")
              .OnceAsync<UserBase>();

            return users?
              .Where(u=>u.Key != userId).Select(x => x.Object);
        }
        public static async Task UpdateUserAsync(string userId, User user)
        {
            await firebaseClient
              .Child("Users")
              .Child(userId)
              .PutAsync(user);
        }
        public static async Task RemoveUserAsync(string userId)
        {
            await firebaseClient
              .Child("Users")
              .Child(userId)
              .DeleteAsync();
        }
        public static async Task<FirebaseObject<User>?> FindUserByEmailAsync(string email)
        {
            var users = await firebaseClient
              .Child("Users")
              .OnceAsync<User>();

            return users?
              .FirstOrDefault(user => user.Object.Email == email);
        }
        public static async Task<User?> FindUserByIdAsync(string userId)
        {
            var user = await firebaseClient
              .Child("Users")
              .Child(userId).OnceSingleAsync<User>();

            return user;
        }
        public static async Task<bool> AddFriendAsync(string senderId, string recipientId)
        {
            Friend recipient = new Friend();
            recipient.UserId = senderId;
            Friend sender = new Friend();
            sender.UserId = recipientId;

            await UpdateFriendAsync(recipientId, senderId, recipient);
            await UpdateFriendAsync(senderId, recipientId, sender);

            return true;
        }
        public static async Task<Friend?> FindFriendAsync(string senderId, string recipientId)
        {
            return await firebaseClient
              .Child($"Friends/{senderId}")
              .Child(recipientId)
              .OnceSingleAsync<Friend>();
        }
        private static async Task UpdateFriendAsync(string senderId, string recipientId, Friend friend)
        {
            await firebaseClient
              .Child("Friends")
              .Child(senderId)
              .Child(recipientId)
              .PutAsync(friend);
        }
        public static async Task RemoveFriendAsync(string senderId, string recipientId)
        {
            await firebaseClient
              .Child("Friends")
              .Child(senderId)
              .Child(recipientId)
              .DeleteAsync();
            await firebaseClient
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
            var friends = await firebaseClient
              .Child($"Friends/{id}")
              .OnceAsync<Friend>();

            return friends?
              .Select(x => x.Object);
        }

        public static async Task<FirebaseObject<Group>> AddGroupAsync(Group group)
        {
            return await firebaseClient
              .Child("Groups")
              .PostAsync(group);
        }
        public static async Task<IEnumerable<Group>?> GetGroupsAsync()
        {
            var groups = await firebaseClient
              .Child("Groups")
              .OnceAsync<Group>();

            return groups?.Select(x => x.Object);
        }
        public static async Task<Group?> GetGroupByIdAsync(string id)
        {
            var groupStr = await firebaseClient
                .Child($"Groups/{id}").OnceAsJsonAsync();
            return JsonConvert.DeserializeObject<Group>(groupStr);
        }
        public static async Task UpdateGroupAsync(string groupId, Group group)
        {
            await firebaseClient
              .Child("Groups")
              .Child(groupId)
              .PutAsync(group);
        }
        public static async Task UpdateMessageAsync(string senderId, string recipientId, string messageId, Message message)
        {
            await firebaseClient
             .Child($"Messages/{senderId}/{recipientId}/{messageId}")
             .PutAsync(message);
        }
        public static async Task<IEnumerable<Dialog>> GetDialogs(string userId)
        {
            var dialogs = await firebaseClient.Child($"Messages/{userId}")
                .OnceAsync<IDictionary<string, Message>>();

            var users = await GetUsersAsync(userId);
            var result = dialogs.Select(x => new Dialog() { User = users?.FirstOrDefault(u => u.Id == x.Key), LastMessage = x.Object.LastOrDefault().Value });
            return result;
        }
        public static async Task RemoveDialogAsync(string userId, string dialogId)
        {
            await firebaseClient
             .Child($"Messages/{userId}/{dialogId}")
             .DeleteAsync();
        }
        public static async Task<IEnumerable<Message>> GetMessages(string userId, string friendId)
        {
            var dialogs = await firebaseClient.Child($"Messages/{userId}/{friendId}")
                .OnceAsync<Message>();

            var result = dialogs.Select(x => x.Object);

            var unread = dialogs.Where(x => x.Object.Status == MessageStatus.Unread && x.Object.SenderId != userId);
            if (unread?.Count() > 0)
            {
                foreach (var message in unread)
                {
                    message.Object.Status = MessageStatus.Read;
                    await UpdateMessageAsync(userId, friendId, message.Key, message.Object);
                    await UpdateMessageAsync(friendId, userId, message.Key, message.Object);
                }
            }

            return result;
        }
        public static async Task<Message?> SendMessageAsync(string senderId, MessageData data)
        {
            Message message = new Message();
            message.Text = data.Text;
            message.CreateTime = DateTime.UtcNow;
            message.SenderId = senderId;
            message.Status = MessageStatus.Unread;

            var result = await firebaseClient
             .Child($"Messages/{data.Id}/{senderId}")
             .PostAsync(message);

            if (result?.Object != null)
            {
                message.Id = result.Key;

                if(data.File != null)
                {
                    string? link = await SaveFileAsync(data.File, message.Id);
                    message.Link = link;
                }

                await UpdateMessageAsync(senderId, data.Id, message.Id, message);
                await UpdateMessageAsync(data.Id, senderId, message.Id, message);
                return message;
            }
            else return null;
        }
        public static async Task<string?> SaveFileAsync(IFormFile file, string name)
        {
            var stream = file.OpenReadStream();
            var task = await firebaseStorage
                 .Child("Messages")
                 .Child(name + ".png")
                 .PutAsync(stream);
            return task;
        }
    }
}
