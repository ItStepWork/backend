using Firebase.Database;
using Firebase.Database.Query;
using backend.Models;
using static System.Net.Mime.MediaTypeNames;

namespace backend.Services
{
    public class PostService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
        public async Task<string> CreatePostAsync(Post model)
        {
            var post = new Post
            {
                Text = model.Text,
                ImageUrl = model.ImageUrl,
                VideoUrl = model.VideoUrl,
                UserId= model.UserId
            };

            var postResponse = await firebaseDatabase.Child("Posts").Child(post.UserId).PostAsync(post);

            return postResponse.Key;
        }
        public async Task<string> CreatePutAsync(Post model)
        {
            var Post = new Post
            {
                Id = model.Id,
            };

            var putResponse = await firebaseDatabase.Child("Puts").PostAsync(put);

            return putResponse.Key;
        }
        public Post GetPostById(string postId)
        {
            var post = firebaseDatabase.Child("Posts").Child(postId).OnceSingleAsync<Post>().Result;
            return post;
        }

        public async Task AddCommentAsync(string senderId, string userId, string postId, string text)
        {
            var post = await firebaseDatabase
              .Child("Posts")
              .Child(userId)
              .Child(postId).OnceSingleAsync<Photo>();

            Comment comment = new();
            comment.SenderId = senderId;
            comment.Text = text;
            comment.CreateTime = DateTime.UtcNow;
            comment.Id = Guid.NewGuid().ToString("N");

            post.Comments.Add(comment.Id, comment);

            await firebaseDatabase
              .Child("Posts")
              .Child(userId)
              .Child(postId)
              .PutAsync(post);
        }

        public async Task LikePostAsync(string senderId, string userId, string postId)
        {
            var post = await firebaseDatabase
              .Child("Posts")
              .Child(userId)
              .Child(postId).OnceSingleAsync<Post>();

            if (post.Likes.Contains(senderId)) post.Likes.Remove(senderId);
            else post.Likes.Add(senderId);

            await firebaseDatabase
              .Child("Posts")
              .Child(userId)
              .Child(postId)
              .PutAsync(post);
        }
    }
}
