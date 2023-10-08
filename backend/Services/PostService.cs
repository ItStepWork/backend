using Firebase.Database;
using Firebase.Database.Query;
using backend.Models;
using Newtonsoft.Json;
using backend.Models.Enums;
using backend.Controllers;

namespace backend.Services
{
    public static class PostService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
        public static async Task<IEnumerable<Post>?> GetPostsAsync(string userId)
        {
            var result = await firebaseDatabase
              .Child($"Posts/{userId}")
              .OnceAsync<Post>();
            var posts = result?.Select(x => x.Object).Where(p => p.Status == Status.Active).OrderByDescending(p => p.CreateTime);
            var users = await UserService.GetUsersAsync();
            var finish = posts?.Select(p => {
                p.Sender = users?.FirstOrDefault(u => u.Id == p.SenderId);
                if (p.RecipientId != null) p.Recipient = users?.FirstOrDefault(u => u.Id == p.RecipientId);
                return p;
            });
            return finish;
        }
        public static async Task<IEnumerable<Post>?> GetPostsAsync(string senderId, string userId)
        {
            if(senderId == userId)
            {
                var friends = await FriendService.GetConfirmedFriends(userId);
                var allGroups = await GroupService.GetGroupsAsync(); 
                if(friends != null)
                {
                    friends.Add(userId);
                    var posts = await firebaseDatabase
                      .Child($"Posts")
                      .OnceAsync<Dictionary<string, Post>>();
                    var userGroups = allGroups?.Where(g => g.Users.ContainsKey(userId) && g.Users[userId] == true).Select(g=>g.Id).ToList();
                    if (userGroups == null) userGroups = new();
                    var result = posts.Where(u => friends.Contains(u.Key) || userGroups.Contains(u.Key)).SelectMany(u => u.Object.Values.ToList()).Where(p => p.Status == Status.Active).OrderByDescending(p => p.CreateTime);
                    var users = await UserService.GetUsersAsync();
                    var groups = await GroupService.GetGroupsAsync();
                    var finish = result.Select(p => {
                        p.Sender = users?.FirstOrDefault(u => u.Id == p.SenderId);  
                        if(p.RecipientId != null) p.Recipient = users?.FirstOrDefault(u => u.Id == p.RecipientId);
                        if (p.GroupId != null) p.Group = groups?.FirstOrDefault(g => g.Id == p.GroupId);
                        return p;
                    });
                    return finish;
                }
                else return await GetPostsAsync(userId);
            }
            else return await GetPostsAsync(userId);
        }
        public static async Task<Post?> GetPostAsync(string userId, string postId)
        {
            var result = await firebaseDatabase
              .Child($"Posts/{userId}/{postId}")
              .OnceAsJsonAsync();

            return JsonConvert.DeserializeObject<Post>(result);
        }
        public static async Task CreatePostAsync(string senderId, Request request)
        {
            string? id = string.IsNullOrEmpty(request.RecipientId) ? request.GroupId : request.RecipientId;

            var post = new Post
            {
                Id = Guid.NewGuid().ToString("N"),
                Text = request.Text,
                SenderId = senderId,
                RecipientId = request.RecipientId,
                GroupId = request.GroupId,
                CreateTime = DateTime.UtcNow,
                Status = Status.Active,
            };

            if (request.File != null)
            {
                string? imgUrl = await UserService.SaveFileAsync(request.File, "Posts", post.Id);
                post.ImgUrl = imgUrl;
            }

            await firebaseDatabase.Child("Posts").Child(id).Child(post.Id).PutAsync(post);
        }
        public static async Task SendCommentAsync(string senderId, Request request)
        {
            var post = await firebaseDatabase
              .Child("Posts")
              .Child(request.RecipientId)
              .Child(request.Id)
              .OnceSingleAsync<Post>();

            Comment comment = new();
            comment.SenderId = senderId;
            comment.Text = request.Text;
            comment.CreateTime = DateTime.UtcNow;
            comment.Id = Guid.NewGuid().ToString("N");

            post.Comments.Add(comment.Id, comment);

            await firebaseDatabase
              .Child("Posts")
              .Child(request.RecipientId)
              .Child(request.Id)
              .PutAsync(post);
        }

        public static async Task SetLikeAsync(string senderId, Request request)
        {
            var post = await firebaseDatabase
              .Child("Posts")
              .Child(request.RecipientId)
              .Child(request.Id)
              .OnceSingleAsync<Post>();

            if (post.Likes.Contains(senderId)) post.Likes.Remove(senderId);
            else post.Likes.Add(senderId);

            await firebaseDatabase
              .Child("Posts")
              .Child(request.RecipientId)
              .Child(request.Id)
              .PutAsync(post);
        }
        public static async Task UpdatePostStatusAsync(string recipientId, string postId, Status status)
        {
            await firebaseDatabase
              .Child("Posts")
              .Child(recipientId)
              .Child(postId)
              .Child("Status")
              .PutAsync<int>((int)status);
        }
    }
}
