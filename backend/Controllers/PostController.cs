using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PostController : Controller
    {
        private readonly PostService _postService;
        private readonly Post _model;

        public PostController(PostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return Ok();
        }

        [HttpPost]
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

        [HttpPost]
        public async Task<IActionResult> AddComment(string senderId, string userId, string postId, string text)
        {
            await _postService.AddCommentAsync(senderId, userId, postId, text);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> LikePost(string senderId, string userId, string postId)
        {
            await _postService.LikePostAsync(senderId, userId, postId);
            return Ok();
        }
    }
}
