using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GalleryController : Controller
    {
        [HttpGet("GetPhotos")]
        public async Task<ActionResult> GetPhotos(string userId)
        {
            var photos = await GalleryService.GetPhotosAsync(userId);
            return Ok(photos);
        }
        [HttpGet("GetPhoto")]
        public async Task<ActionResult> GetPhoto(string userId, string photoId)
        {
            var result = await GalleryService.GetPhotoAsync(userId, photoId);
            return Ok(result);
        }
        [HttpGet("GetAlbumPhotos")]
        public async Task<ActionResult> GetAlbumPhotos(string userId, string albumId)
        {
            var result = await GalleryService.GetPhotosAsync(userId, albumId);
            return Ok(result);
        }
        [HttpPost("AddPhoto")]
        public async Task<ActionResult> AddPhoto(IFormFile file)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var photo = await GalleryService.AddPhotoAsync(userId);

            var url = await UserService.SaveFileAsync(file, "Photos", photo.Key);
            if (url == null) return Conflict("Save photo failed");

            Photo result = photo.Object;
            result.Id = photo.Key;
            result.Url = url;

            await GalleryService.UpdatePhotoAsync(userId, photo.Key, result);
            return Ok("Ok");
        }
        [HttpPost("SendCommentPhoto")]
        public async Task<ActionResult> SendCommentPhoto(Request request)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            await GalleryService.SendCommentPhotoAsync(userId, request.UserId, request.PhotoId, request.Text);
            return Ok("Ok");
        }
        [HttpPost("SetLikePhoto")]
        public async Task<ActionResult> SetLikePhoto(Request request)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            await GalleryService.SetLikePhotoAsync(userId, request.UserId, request.PhotoId);
            return Ok("Ok");
        }
        [HttpPost("SetAvatar")]
        public async Task<ActionResult> SetAvatar(Request request)
        {
            if (string.IsNullOrEmpty(request.Url)) return BadRequest("Url is null or empty");
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            await UserService.UpdateUserAvatarUrlAsync(userId, request.Url);
            return Ok("Ok");
        }
        [HttpPost("SetBackground")]
        public async Task<ActionResult> SetBackground(Request request)
        {
            if (string.IsNullOrEmpty(request.Url)) return BadRequest("Url is null or empty");
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            await UserService.UpdateUserBackgroundUrlAsync(userId, request.Url);
            return Ok("Ok");
        }
        [HttpPost("SetAlbum")]
        public async Task<ActionResult> SetAlbum(Request request)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            if (string.IsNullOrEmpty(request.PhotoId) || request.AlbumId == null) return BadRequest("Wrong data");
            var result = await GalleryService.GetPhotoAsync(userId, request.PhotoId);
            if (result == null) return NotFound("Photo not found");
            result.AlbumId = request.AlbumId;
            await GalleryService.UpdatePhotoAsync(userId, request.PhotoId, result);
            return Ok("Ok");
        }
        [HttpDelete("RemovePhoto")]
        public async Task<ActionResult> RemovePhoto(string id)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            await GalleryService.RemovePhotoAsync(userId, id);
            return Ok("Ok");
        }
        [HttpGet("GetAlbums")]
        public async Task<ActionResult> GetAlbums(string userId)
        {
            var result = await GalleryService.GetAlbumsAsync(userId);
            return Ok(result);
        }
        [HttpPost("AddAlbum")]
        public async Task<ActionResult> AddAlbum([FromForm]Request request)
        {
            if (string.IsNullOrEmpty(request.Name)) return BadRequest("Name is null or empty");

            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var album = await GalleryService.AddAlbumAsync(userId);

            if(request?.Files?.Length > 0)
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
                        photo.AlbumId = album.Key;
                        await GalleryService.UpdatePhotoAsync(userId, id, photo);
                    }
                }
            }

            album.Object.Id = album.Key;
            album.Object.Name = request.Name;
            await GalleryService.UpdateAlbumAsync(userId, album.Key, album.Object);
            return Ok("Ok");
        }
        [HttpDelete("RemoveAlbum")]
        public async Task<ActionResult> RemoveAlbum(string id)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            await GalleryService.RemoveAlbumAsync(userId, id);
            return Ok("Ok");
        }
        [HttpDelete("RemoveAlbumAndPhotos")]
        public async Task<ActionResult> RemoveAlbumAndPhotos(string id)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var result = await GalleryService.GetPhotosAsync(userId, id);
            if(result == null || result.Count() == 0) return NotFound("Photos not found");
            foreach (var photo in result)
            {
                await GalleryService.RemovePhotoAsync(userId, photo.Id);
            }
            await GalleryService.RemoveAlbumAsync(userId, id);
            return Ok("Ok");
        }
    }
}
