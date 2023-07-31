using backend.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace backend.Services
{
    public static class UserService
    {
        private static string firebaseDatabaseUrl = "https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/";
        private static readonly FirebaseClient firebaseClient = new FirebaseClient(firebaseDatabaseUrl);

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
        public static async Task<IEnumerable<Group>?> GetGroupsAsync(string userId)
        {
            var groups = await firebaseClient
              .Child("Groups")
              .OnceAsync<Group>();

            return groups?.Select(x => x.Object);
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
        public static async Task<Message?> SendMessageAsync(string senderId, string recipientId, string text)
        {
            Message message = new Message();
            message.Text = text;
            message.CreateTime = DateTime.UtcNow;
            message.SenderId = senderId;
            message.Status = MessageStatus.Unread;

            var result = await firebaseClient
             .Child($"Messages/{recipientId}/{senderId}")
             .PostAsync(message);

            if (result?.Object != null)
            {
                message.Id = result.Key;

                await UpdateMessageAsync(senderId, recipientId, message.Id, message);
                await UpdateMessageAsync(recipientId, senderId, message.Id, message);
                return message;
            }
            else return null;
        }

    }
}
