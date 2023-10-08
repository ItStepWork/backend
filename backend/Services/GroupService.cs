using backend.Models;
using backend.Models.Enums;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using System.Security.Claims;

namespace backend.Services
{
    public class GroupService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
        private static readonly FirebaseStorage firebaseStorage = new FirebaseStorage("database-50f39.appspot.com");
        public static async Task<(string response, Group? group)> AddGroupAsync(Request groupRequest, string userId)
        {
            UserBase? user = await UserService.GetUserAsync(userId);
            if (user == null) return ("Admin user not found", null);
            Group group = new Group();
            group.Status = Status.Active;
            group.CreatedTime = DateTime.UtcNow;
            group.Name = groupRequest.Name;
            group.Description = groupRequest.Description;
            group.Audience = (Audience)groupRequest.Audience;
            group.AdminId = userId;
            group.Email = user.Email;
            group.Users.Add(userId, true);
            var result = await firebaseDatabase
              .Child("Groups")
              .PostAsync(group);
            if (result == null) return ("Group not added", null);
            group.Id = result.Key;
            var url = await UserService.SaveFileAsync(groupRequest.File, "Groups", group.Id);
            if (url == null) return ("Avatar not saved", null);
            group.PictureUrl = url;
            await UpdateGroupAsync(group);
            return ("", group);
        }
        public static async Task UpdateGroupAsync(Group group)
        {
            await firebaseDatabase
              .Child("Groups")
              .Child(group.Id)
              .PutAsync(group);
        }
        public static async Task RemuveGroupAsync(string groupId)
        {
            await firebaseDatabase
              .Child("Groups")
              .Child(groupId)
              .Child("Status")
              .PutAsync<int>((int)Status.Deleted);
        }
        public static async Task<IEnumerable<Group>?> GetGroupsAsync()
        {
            var groups = await firebaseDatabase
              .Child("Groups")
              .OnceAsync<Group>();
            return groups?.Select(x => x.Object).Where(g=>g.Status == Status.Active);
        }
        public static async Task<Group?> GetGroupAsync(string id)
        {
            var group = await firebaseDatabase
              .Child($"Groups")
              .Child(id).OnceSingleAsync<Group>();
            if (group?.Status == Status.Active) return group;
            else return null;
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
        public static async Task<(string response, IEnumerable<Friend>? friends)> GetFriendsInGroup(string groupId, string myId)
        {
            var group = await GetGroupAsync(groupId);
            if (group != null)
            {
                var users = group?.Users?.Where((a) => a.Value == true).Select((a) => a.Key).ToList();
                if (users != null)
                {
                    var friends = await FriendService.GetFriendsAsync(myId, myId);
                    var result = friends?.Where(f => users.Contains(f.Id)).ToList();
                    result?.Sort((left, right) => left.FirstName == right.FirstName ? left.LastName.CompareTo(right.LastName) : left.FirstName.CompareTo(right.FirstName));
                    result?.Sort((y, x) => Convert.ToInt32(x?.Id?.Equals(group?.AdminId)) - Convert.ToInt32(y?.Id?.Equals(group?.AdminId)));
                    if (users.Contains(myId))
                    {
                        var my = await FriendService.GetFriendAsync(myId);
                        result.Insert(0, my);
                    }
                    return ("", result);
                }
                return ("Users is empty", null);
            }
            return ("Group not found", null);
        }
        public static async Task<(string response, IEnumerable<Friend>? friends)> GetFriendsForInvitation(string groupId, string myId)
        {
            var group = await GetGroupAsync(groupId);
            if (group != null)
            {
                var users = group?.Users?.Select((a) => a.Key).ToList();
                if (users != null)
                {
                    var friends = await FriendService.GetFriendsAsync(myId, myId);
                    var result = friends?.Where(f => f.FriendStatus == FriendStatus.Confirmed && !users.Contains(f.Id)).ToList();
                    result?.Sort((left, right) => left.FirstName == right.FirstName ? left.LastName.CompareTo(right.LastName) : left.FirstName.CompareTo(right.FirstName));
                    return ("", result);
                }
                return ("Users is empty", null);
            }
            return ("Group not found", null);


        }
        public static async Task<(string response, IEnumerable<UserBase>? users)> GetMembersGroup(string groupId)
        {
            var group = await GetGroupAsync(groupId);
            if (group != null)
            {
                var users = group?.Users?.Where((a) => a.Value == true).Select((a) => a.Key).ToList();
                if (users != null)
                {
                    var result = await UserService.GetUsersAsync(users.ToArray());
                    result?.Sort((left, right) => left.FirstName == right.FirstName ? left.LastName.CompareTo(right.LastName) : left.FirstName.CompareTo(right.FirstName));
                    result?.Sort((y, x) => Convert.ToInt32(x?.Id?.Equals(group?.AdminId)) - Convert.ToInt32(y?.Id?.Equals(group?.AdminId)));
                    return ("", result);
                }
                return ("Users is empty", null);
            }
            return ("Group not found", null);
        }
        public static async Task<(string response, IEnumerable<UserBase>? users)> GetRequestsToGroup(string groupId)
        {
            var group = await GetGroupAsync(groupId);
            if (group != null)
            {
                var users = group?.Users?.Where((a) => a.Value == false).Select((a) => a.Key);
                if (users != null)
                {
                    var result = await UserService.GetUsersAsync(users.ToArray());
                    result?.Sort((left, right) => left.FirstName == right.FirstName ? left.LastName.CompareTo(right.LastName) : left.FirstName.CompareTo(right.FirstName));
                    result?.Sort((y, x) => Convert.ToInt32(x?.Id?.Equals(group?.AdminId)) - Convert.ToInt32(y?.Id?.Equals(group?.AdminId)));
                    return ("", result);
                }
                return ("Users is empty", null);
            }
            return ("Group not found", null);
        }
        public static async Task<(string response, string ok)> AddPhoto(Request request, string myId)
        {
            var group = await GetGroupAsync(request.Id);
            if (group != null)
            {
                if (group.AdminId != myId) return ("You not Admin", "");

                var photo = await GalleryService.AddPhotoAsync(request.Id);

                var url = await UserService.SaveFileAsync(request.File, "Groups", photo.Key);
                if (url == null) return ("Save photo failed","");

                Photo result = photo.Object;
                result.Id = photo.Key;
                result.Url = url;

                await GalleryService.UpdatePhotoAsync(request.Id, photo.Key, result);

                return ("","Ok");
            }
            return ("Group not found", "");
        }
        public static async Task<(string response, string ok)> RemovePhoto(Request request, string myId)
        {
            var group = await GetGroupAsync(request.Id);
            if (group != null)
            {
                if (group.AdminId != myId) return ("You not Admin", "");

                await GalleryService.RemovePhotoAsync(request.Id, request.PhotoId);
                await UserService.RemoveFileAsync("Groups", request.PhotoId);
                return ("", "Ok");
            }
            return ("Group not found", "");
        }
           
        

    }
}
