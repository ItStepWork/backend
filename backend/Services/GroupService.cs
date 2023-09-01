using backend.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using Newtonsoft.Json;

namespace backend.Services
{
    public class GroupService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
        private static readonly FirebaseStorage firebaseStorage = new FirebaseStorage("database-50f39.appspot.com");
        public static async Task<FirebaseObject<Group>> AddGroupAsync(Group group)
        {
            return await firebaseDatabase
              .Child("Groups")
              .PostAsync(group);
        }
        public static async Task RemuveGroupAsync(string groupId)
        {
            await firebaseDatabase
              .Child("Groups")
              .Child(groupId)
              .DeleteAsync();
            await UserService.RemoveFileAsync("Groups", groupId);
            var photos = await GalleryService.GetPhotosAsync(groupId);
            await GalleryService.RemovePhotosFolderAsync(groupId);
            foreach (var item in photos)
            {
                await UserService.RemoveFileAsync("Photos", item.Id);
            }

        }
        public static async Task<IEnumerable<Group>?> GetGroupsAsync()
        {
            var groups = await firebaseDatabase
              .Child("Groups")
              .OnceAsync<Group>();

            return groups?.Select(x => x.Object);
        }
        public static async Task<Group> GetGroupAsync(string id)
        {
            var group = await firebaseDatabase
              .Child($"Groups")
              .Child(id).OnceSingleAsync<Group>();
            return group;
        }
        public static async Task<Group?> GetGroupByIdAsync(string id)
        {
            var groupStr = await firebaseDatabase
                .Child($"Groups/{id}").OnceAsJsonAsync();
            return JsonConvert.DeserializeObject<Group>(groupStr);
        }
        public static async Task UpdateGroupAsync(string groupId, Group group)
        {
            await firebaseDatabase
              .Child("Groups")
              .Child(groupId)
              .PutAsync(group);
        }
        public static async Task RemuveUserFromGroupAsync(string groupId, string userId)
        {
            await firebaseDatabase
              .Child("Groups")
              .Child(groupId)
              .Child("Users")
              .Child(userId)
              .DeleteAsync();
        }
       
    }
}
