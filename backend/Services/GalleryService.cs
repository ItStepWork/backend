using backend.Models;
using backend.Models.Enums;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;

namespace backend.Services
{
    public static class GalleryService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");

        public static async Task<IEnumerable<Photo>?> GetPhotosAsync(string userId)
        {
            var result = await firebaseDatabase
              .Child($"Photos/{userId}")
              .OnceAsync<Photo>();

            var photos = result?.Select(x => x.Object);
            if (photos == null) return new Photo[] { };

            foreach (var photo in photos)
            {
                var sorted = photo.Comments.OrderBy(x => x.Value.CreateTime).ToDictionary(x => x.Key, x => x.Value);
                photo.Comments = sorted;
            }

            return photos;
        }
        public static async Task<IEnumerable<Photo>?> GetPhotosAsync(string userId, string albumId)
        {
            var result = await firebaseDatabase
              .Child($"Photos/{userId}")
              .OnceAsync<Photo>();

            var photos = result?.Where(x => x.Object.AlbumId == albumId).Select(x => x.Object);
            if (photos == null) return new Photo[] { };

            foreach (var photo in photos)
            {
                var sorted = photo.Comments.OrderBy(x => x.Value.CreateTime).ToDictionary(x => x.Key, x => x.Value);
                photo.Comments = sorted;
            }

            return photos;
        }
        public static async Task<IEnumerable<Photo>?> GetStoryPhotosAsync(string userId, string storyId)
        {
            var result = await firebaseDatabase
              .Child($"Photos/{userId}")
              .OnceAsync<Photo>();

            var photos = result?.Where(x => x.Object.StoryId == storyId).Select(x => x.Object);
            return photos;
        }
        public static async Task<Photo?> GetPhotoAsync(string userId, string photoId)
        {
            var result = await firebaseDatabase
              .Child("Photos")
              .Child(userId)
              .Child(photoId)
              .OnceSingleAsync<Photo>();

            var sorted = result.Comments.OrderBy(x => x.Value.CreateTime).ToDictionary(x => x.Key, x => x.Value);
            result.Comments = sorted;

            return result;
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
        public static async Task RemovePhotosFolderAsync(string userId)
        {
            await firebaseDatabase
              .Child($"Photos/{userId}")
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
            await NotificationService.AddNotificationAsync(senderId, userId, NotificationType.CommentPhoto);
        }
        public static async Task SetLikePhotoAsync(string senderId, string userId, string photoId)
        {
            var photo = await firebaseDatabase
              .Child("Photos")
              .Child(userId)
              .Child(photoId).OnceSingleAsync<Photo>();

            if (photo.Likes.Contains(senderId)) photo.Likes.Remove(senderId);
            else
            {
                photo.Likes.Add(senderId);
                await NotificationService.AddNotificationAsync(senderId, userId, NotificationType.LikePhoto);
            }

            await firebaseDatabase
              .Child("Photos")
              .Child(userId)
              .Child(photoId)
              .PutAsync(photo);
        }
        public static async Task<IEnumerable<Album>?> GetAlbumsAsync(string userId)
        {
            var result = await firebaseDatabase
              .Child($"Albums/{userId}")
              .OnceAsync<Album>();

            return result?.Select(x => x.Object);
        }
        public static async Task<FirebaseObject<Album>> AddAlbumAsync(string userId)
        {
            return await firebaseDatabase
              .Child($"Albums/{userId}")
              .PostAsync(new Album());
        }
        public static async Task UpdateAlbumAsync(string userId, string albumId, Album album)
        {
            await firebaseDatabase
              .Child($"Albums/{userId}/{albumId}")
              .PutAsync(album);
        }
        public static async Task RemoveAlbumAsync(string userId, string albumId)
        {
            await firebaseDatabase
              .Child($"Albums/{userId}/{albumId}")
              .DeleteAsync();
        }
    }
}
