using backend.Models;
using backend.Models.Enums;
using Firebase.Database;
using Firebase.Database.Query;

namespace backend.Services
{
    public static class SupportService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");

        public static async Task SendMessageAsync(string senderId, Request request)
        {
            Message message = new Message();
            message.Id = Guid.NewGuid().ToString("N");
            message.Text = request.Text;
            message.CreateTime = DateTime.UtcNow;
            message.SenderId = senderId;
            message.Status = MessageStatus.Unread;

            if (request.File != null)
            {
                string? link = await UserService.SaveFileAsync(request.File, "Support", message.Id);
                message.Link = link;
            }

            await firebaseDatabase
             .Child($"Support/Messages/{senderId}/{message.Id}")
             .PutAsync(message);
        }
        public static async Task<IEnumerable<Message>?> GetMessagesAsync(string userId)
        {
            var result = await firebaseDatabase.Child($"Support/Messages/{userId}")
                .OnceAsync<Message>();

            return result?.Select(x => x.Object).OrderBy(m=>m.CreateTime);
        }
        public static async Task SendComplaintAsync(string senderId, Request request)
        {
            Complaint complaint = new Complaint();
            complaint.Id = Guid.NewGuid().ToString("N");
            complaint.Text = request.Text;
            complaint.CreateTime = DateTime.UtcNow;
            complaint.SenderId = senderId;
            complaint.Status = MessageStatus.Unread;
            complaint.UserId = request.UserId;
            complaint.PhotoId = request.PhotoId;
            complaint.PhotoUrl = request.PhotoUrl;

            if (request.File != null)
            {
                string? link = await UserService.SaveFileAsync(request.File, "Complaints", complaint.Id);
                complaint.Link = link;
            }

            await firebaseDatabase
             .Child($"Complaints/{complaint.Id}")
             .PutAsync(complaint);
        }
    }
}
