using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PostController : Controller
    {
        [Authorize]
        [HttpGet("GetPosts")]
        public async Task<ActionResult> GetPosts(string id)
        {
            var result = await PostService.GetPostsAsync(id);
            return Ok(result);
        }

        [HttpPost("CreatePost")]
        public async Task<ActionResult> CreatePost([FromForm] Request request)
        {
            if (string.IsNullOrEmpty(request.RecipientId) || (string.IsNullOrEmpty(request.Text) && request.File == null)) return BadRequest("Data in null or empty");
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");
            await PostService.CreatePostAsync(userId, request);
            return Ok("Ok");
        }

        [HttpPost("SendComment")]
        public async Task<ActionResult> SendComment(Request request)
        {
            if (string.IsNullOrEmpty(request.Id) || string.IsNullOrEmpty(request.RecipientId) || string.IsNullOrEmpty(request.Text)) return BadRequest("Data in null or empty");
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");
            await PostService.SendCommentAsync(userId, request);
            return Ok("Ok");
        }

        [HttpPost("SetLike")]
        public async Task<ActionResult> SetLike(Request request)
        {
            if (string.IsNullOrEmpty(request.Id) || string.IsNullOrEmpty(request.RecipientId)) return BadRequest("Data in null or empty");
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");
            await PostService.SetLikeAsync(userId, request);
            return Ok("Ok");
        }

        [HttpDelete("RemovePost")]
        public async Task<ActionResult> RemovePost(string id)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var result = await PostService.GetPostAsync(userId, id);
            if (result == null) return Conflict("No access"); 
            await PostService.RemovePostAsync(userId, id);
            return Ok("Ok");
        }
    }
}
