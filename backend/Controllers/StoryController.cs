using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoryController : Controller
    {

        [HttpGet("GetStoryPhotos")]
        public async Task<ActionResult> GetStoryPhotos(string userId, string storyId)
        {
            var result = await GalleryService.GetStoryPhotosAsync(userId, storyId);
            return Ok(result);
        }

        [HttpGet("GetStories")]
        public async Task<ActionResult> GetStories(string userId)
        {
            var result = await StoryService.GetStoriesAsync(userId);
            return Ok(result);
        }

        [HttpPost("AddStory")]
        public async Task<ActionResult> AddStory([FromForm]Request request)
        {

            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var story = await StoryService.AddStoryAsync(userId);

            if (request?.Files?.Length > 0)
            {
                foreach (var file in request.Files)
                {
                    string id = Guid.NewGuid().ToString("N");
                    var url = await UserService.SaveFileAsync(file, "Photos", id);
                    if (url != null)
                    {
                        Photo photo = new();
                        photo.Id = id;
                        photo.Url = url;
                        photo.StoryId = story.Key;
                        await GalleryService.UpdatePhotoAsync(userId, id, photo);
                    }
                }
            }
            story.Object.Id = story.Key;
            story.Object.Name = request?.Name;
            story.Object.CreatedTime = DateTime.UtcNow;
            await StoryService.UpdatStoryAsync(userId, story.Key, story.Object);
            return Ok("Ok");
        }
    }
}
