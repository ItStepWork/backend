using Firebase.Database;
using Firebase.Database.Query;
using backend.Models;

namespace backend.Services
{
    public class PostService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
        public async Task<string> CreatePostAsync(CreatePostViewModel model)
        {
            var post = new Post
            {
                Text = model.Text,
                ImageUrl = model.ImageUrl,
                VideoUrl = model.VideoUrl,
                Likes = 0,
                Reposts = 0,
                Comments = new List<Comment>()
            };

            var postResponse = await firebaseDatabase.Child("Posts").PostAsync(post);

            return postResponse.Key;
        }

        public Post GetPostById(string postId)
        {
            var post = firebaseDatabase.Child("Posts").Child(postId).OnceSingleAsync<Post>().Result;
            return post;
        }

        public async Task AddCommentAsync(string postId, string commentText)
        {
            var comment = new Comment
            {
                Text = commentText,
                CreateTime = DateTime.UtcNow
            };

            await firebaseDatabase.Child("Posts").Child(postId).Child("Comments").PostAsync(comment);
        }

        public async Task LikePostAsync(string postId)
        {
            var post = GetPostById(postId);
            post.Likes++;

            await firebaseDatabase.Child("Posts").Child(postId).PutAsync(post);
        }

        public async Task<string> RepostAsync(string postId)
        {
            var post = GetPostById(postId);

            var newPost = new Post
            {
                Text = post.Text,
                ImageUrl = post.ImageUrl,
                VideoUrl = post.VideoUrl,
                Likes = 0,
                Reposts = 0,
                Comments = new List<Comment>()
            };

            var newPostResponse = await firebaseDatabase.Child("Posts").PostAsync(newPost);

            return newPostResponse.Key;
        }
    }
}
