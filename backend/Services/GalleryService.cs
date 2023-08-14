using backend.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace backend.Services
{
    public static class GalleryService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");

        public static async Task<IEnumerable<Photo>?> GetPhotosAsync(string userId)
        {
            var users = await firebaseDatabase
              .Child($"Photos/{userId}")
              .OnceAsync<Photo>();

            return users?.Select(x => x.Object);
        }
        public static async Task<FirebaseObject<Photo>> AddPhotoAsync(string userId)
        {
            return await firebaseDatabase
              .Child($"Photos/{userId}")
              .PostAsync(new Photo());
        }
        public static async Task UpdatePhotoAsync(string userId, string photoId, Photo photo)
        {
            await firebaseDatabase
              .Child($"Photos/{userId}/{photoId}")
              .PutAsync(photo);
        }
        public static async Task RemovePhotoAsync(string userId, string photoId)
        {
            await firebaseDatabase
              .Child($"Photos/{userId}/{photoId}")
              .DeleteAsync();
        }
        public static async Task SendCommentPhotoAsync(string senderId, string userId, string photoId, string text)
        {
            var photo = await firebaseDatabase
              .Child("Photos")
              .Child(userId)
              .Child(photoId).OnceSingleAsync<Photo>();

            Comment comment = new();
            comment.SenderId = senderId;
            comment.Text = text;
            comment.CreateTime = DateTime.UtcNow;
            comment.Id = Guid.NewGuid().ToString("N");

            photo.Comments.Add(comment.Id, comment);

            await firebaseDatabase
              .Child("Photos")
              .Child(userId)
              .Child(photoId)
              .PutAsync(photo);
        }
        public static async Task SetLikePhotoAsync(string senderId, string userId, string photoId)
        {
            var photo = await firebaseDatabase
              .Child("Photos")
              .Child(userId)
              .Child(photoId).OnceSingleAsync<Photo>();

            if (photo.Likes.Contains(senderId)) photo.Likes.Remove(senderId);
            else photo.Likes.Add(senderId);

            await firebaseDatabase
              .Child("Photos")
              .Child(userId)
              .Child(photoId)
              .PutAsync(photo);
        }
    }
}
