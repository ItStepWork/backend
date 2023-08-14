using backend.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace backend.Services
{
    public static class MessagingService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");

        public static async Task UpdateMessageAsync(string senderId, string recipientId, string messageId, Message message)
        {
            await firebaseDatabase
             .Child($"Messages/{senderId}/{recipientId}/{messageId}")
             .PutAsync(message);
        }
        public static async Task<IEnumerable<Dialog>> GetDialogs(string userId)
        {
            var dialogs = await firebaseDatabase.Child($"Messages/{userId}")
                .OnceAsync<IDictionary<string, Message>>();

            var users = await UserService.GetUsersAsync(userId);
            var result = dialogs.Select(x => new Dialog() { User = users?.FirstOrDefault(u => u.Id == x.Key), LastMessage = x.Object.LastOrDefault().Value });
            return result;
        }
        public static async Task RemoveDialogAsync(string userId, string dialogId)
        {
            await firebaseDatabase
             .Child($"Messages/{userId}/{dialogId}")
             .DeleteAsync();
        }
        public static async Task<IEnumerable<Message>> GetMessages(string userId, string friendId)
        {
            var dialogs = await firebaseDatabase.Child($"Messages/{userId}/{friendId}")
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

            var result = await firebaseDatabase
             .Child($"Messages/{data.Id}/{senderId}")
             .PostAsync(message);

            if (result?.Object != null)
            {
                message.Id = result.Key;

                if (data.File != null)
                {
                    string? link = await UserService.SaveFileAsync(data.File, "Messages", message.Id);
                    message.Link = link;
                }

                await UpdateMessageAsync(senderId, data.Id, message.Id, message);
                await UpdateMessageAsync(data.Id, senderId, message.Id, message);
                return message;
            }
            else return null;
        }
    }
}
