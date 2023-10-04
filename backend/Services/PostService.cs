using Firebase.Database;
using Firebase.Database.Query;
using backend.Models;
using Newtonsoft.Json;

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

            return result?.Select(x => x.Object);
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
            var post = new Post
            {
                Id = Guid.NewGuid().ToString("N"),
                Text = request.Text,
                SenderId = senderId,
                RecipientId = request.RecipientId,
                CreateTime = DateTime.UtcNow,
            };

            if (request.File != null)
            {
                string? imgUrl = await UserService.SaveFileAsync(request.File, "Posts", post.Id);
                post.ImgUrl = imgUrl;
            }

            await firebaseDatabase.Child("Posts").Child(post.RecipientId).Child(post.Id).PutAsync(post);
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
        public static async Task RemovePostAsync(string senderId, string postId)
        {
            await firebaseDatabase
              .Child("Posts")
              .Child(senderId)
              .Child(postId)
              .DeleteAsync();
        }
    }
}
