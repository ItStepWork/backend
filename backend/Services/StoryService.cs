using backend.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace backend.Services
{
    public static class StoryService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");

        public static async Task<IEnumerable<Story>?> GetStoriesAsync(string userId)
        {
            var result = await firebaseDatabase
              .Child($"Stories/{userId}")
              .OnceAsync<Story>();

            var resultPhotos = await firebaseDatabase
              .Child($"Photos/{userId}")
              .OnceAsync<Photo>();

            var photos = resultPhotos?.Select(x => x.Object);
            var stories = result?.Select(x => x.Object).Select(s => { s.Photos = photos?.Where(p => p.StoryId == s.Id); return s; });
            return stories;
        }

        public static async Task<FirebaseObject<Story>> AddStoryAsync(string userId)
        {
            return await firebaseDatabase
              .Child($"Stories/{userId}")
              .PostAsync(new Story());
        }

        public static async Task RemoveStoryAsync(string userId, string storyId)
        {
            await firebaseDatabase
              .Child($"Stories/{userId}/{storyId}")
              .DeleteAsync();
        }

        public static async Task UpdatStoryAsync(string userId, string storyId, Story story)
        {
            await firebaseDatabase
              .Child($"Stories/{userId}/{storyId}")
              .PutAsync(story);
        }
    }
}
