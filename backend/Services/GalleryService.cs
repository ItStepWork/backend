using backend.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;

namespace backend.Services
{
    public static class GalleryService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
        private static readonly FirebaseStorage firebaseStorage = new FirebaseStorage("database-50f39.appspot.com");

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
    }
}
