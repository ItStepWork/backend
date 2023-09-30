using backend.Models;
using backend.Models.Enums;
using Firebase.Database;
using Firebase.Database.Query;
using MimeKit;
using Org.BouncyCastle.Cms;

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
            var messages = await firebaseDatabase.Child($"Messages/{userId}/{friendId}")
                .OnceAsync<Message>();

            var result = messages.Select(x => x.Object).OrderByDescending(m=>m.CreateTime);

            return result;
        }
        public static async Task UpdateMessageStatusAsync(string senderId, string friendId, string messageId)
        {
            await firebaseDatabase
            .Child($"Messages/{senderId}/{friendId}/{messageId}/Status")
             .PutAsync<int>((int)MessageStatus.Read);
            await firebaseDatabase
            .Child($"Messages/{friendId}/{senderId}/{messageId}/Status")
             .PutAsync<int>((int)MessageStatus.Read);
        }
        public static async Task<Message?> SendMessageAsync(string senderId, Request data)
        {
            Message message = new Message();
            message.Text = data.Text;
            message.CreateTime = DateTime.UtcNow;
            message.SenderId = senderId;
            message.RecipientId = data.Id;
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
        public static async Task RemoveMessageAsync(string userId, string messageId, bool isFull)
        {
            var dialogs = await firebaseDatabase.Child($"Messages/{userId}")
                .OnceAsync<Dictionary<string, Message>>();

            var result = dialogs.Where(dialog => dialog.Object.ContainsKey(messageId)).FirstOrDefault();
            if (result != null)
            {
                var message = result.Object[messageId];
                if (message?.RecipientId != null)
                {
                    if(userId == message.SenderId)
                    {
                        await firebaseDatabase.Child($"Messages/{userId}/{message.RecipientId}/{messageId}").DeleteAsync();
                        if (isFull)
                        {
                            await firebaseDatabase.Child($"Messages/{message.RecipientId}/{userId}/{messageId}").DeleteAsync();
                        }
                    }
                    else
                    {
                        await firebaseDatabase.Child($"Messages/{userId}/{message.SenderId}/{messageId}").DeleteAsync();
                    }
                    
                }
            }
        }
    }
}
