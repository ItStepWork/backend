using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PostController : Controller
    {
        private readonly PostService _postService;

        public PostController(PostService postService)
        {
            _postService = postService;
        }

        [Authorize]
        [HttpGet("GetPost")]
        public async Task<IActionResult> GetPost()
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var post = await PostService.GetPostAsync(resultValidate.user.Id);
            return Ok(post);
        }

        [HttpGet("CreatePost")]
        public IActionResult Create()
        {
            return Ok();
        }

        [HttpPost("CreatePost")]
        public async Task<IActionResult> Create(Post model)
        {
            if (ModelState.IsValid)
            {
                var putId = await _postService.CreatePutAsync(model);
                var postId = await _postService.CreatePostAsync(model);

                return RedirectToAction("Index", "Home");
            }

            return Ok(model);
        }

        [HttpGet]
        public IActionResult Details(string postId)
        {
            var post = _postService.GetPostById(postId);
            return Ok(post);
        }

        [HttpPost("AddComment")]
        public async Task<IActionResult> AddComment(string senderId, string userId, string postId, string text)
        {
            await _postService.AddCommentAsync(senderId, userId, postId, text);
            return Ok();
        }

        [HttpPost("LikePost")]
        public async Task<IActionResult> LikePost(string senderId, string userId, string postId)
        {
            await _postService.LikePostAsync(senderId, userId, postId);
            return Ok();
        }

        private async Task<(string, User?)> ValidationUser()
        {
            Claim? claimId = this.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid);
            if (claimId == null) return ("User not authorize!", null);

            User? sender = await UserService.FindUserByIdAsync(claimId.Value);
            if (sender == null) return ("Sender not found!", null);

            sender.LastVisit = DateTime.UtcNow;
            await UserService.UpdateUserAsync(claimId.Value, sender);
            return ("", sender);
        }
    }
}
