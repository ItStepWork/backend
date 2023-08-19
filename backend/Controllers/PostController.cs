using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;

namespace backend.Controllers
{
    public class PostController : Controller
    {
        private readonly PostService _postService;

        public PostController(PostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            if (ModelState.IsValid)
            {
                var postId = await _postService.CreatePostAsync(model);

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Details(string postId)
        {
            var post = _postService.GetPostById(postId);
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(string senderId, string userId, string postId, string text)
        {
            await _postService.AddCommentAsync(senderId, userId, postId, text);
            return RedirectToAction("Details", new { postId = postId });
        }

        [HttpPost]
        public async Task<IActionResult> LikePost(string senderId, string userId, string postId)
        {
            await _postService.LikePostAsync(senderId, userId, postId);
            return RedirectToAction("Details", new { postId = postId });
        }
    }
}
