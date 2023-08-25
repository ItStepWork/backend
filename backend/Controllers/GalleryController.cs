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
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var photos = await GalleryService.GetPhotosAsync(userId);
            return Ok(photos);
        }
        [HttpGet("GetPhoto")]
        public async Task<ActionResult> GetPhoto(string userId, string photoId)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await GalleryService.GetPhotoAsync(userId, photoId);
            return Ok(result);
        }
        [HttpGet("GetAlbumPhotos")]
        public async Task<ActionResult> GetAlbumPhotos(string userId, string albumId)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await GalleryService.GetPhotosAsync(userId, albumId);
            return Ok(result);
        }
        [HttpPost("AddPhoto")]
        public async Task<ActionResult> AddPhoto(IFormFile file)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var photo = await GalleryService.AddPhotoAsync(resultValidate.user.Id);

            var url = await UserService.SaveFileAsync(file, "Photos", photo.Key);
            if (url == null) return Conflict("Save photo failed");

            Photo result = photo.Object;
            result.Id = photo.Key;
            result.Url = url;

            await GalleryService.UpdatePhotoAsync(resultValidate.user.Id, photo.Key, result);

            return Ok("Ok");
        }
        [HttpPost("SendCommentPhoto")]
        public async Task<ActionResult> SendCommentPhoto(GalleryRequest request)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            await GalleryService.SendCommentPhotoAsync(resultValidate.user.Id, request.UserId, request.PhotoId, request.Text);
            return Ok("Ok");
        }
        [HttpPost("SetLikePhoto")]
        public async Task<ActionResult> SetLikePhoto(GalleryRequest request)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            await GalleryService.SetLikePhotoAsync(resultValidate.user.Id, request.UserId, request.PhotoId);
            return Ok("Ok");
        }
        [HttpPost("SetAvatar")]
        public async Task<ActionResult> SetAvatar(GalleryRequest request)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            resultValidate.user.AvatarUrl = request.Url;
            await UserService.UpdateUserAsync(resultValidate.user.Id, resultValidate.user);
            return Ok("Ok");
        }
        [HttpPost("SetBackground")]
        public async Task<ActionResult> SetBackground(GalleryRequest request)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            resultValidate.user.BackgroundUrl = request.Url;
            await UserService.UpdateUserAsync(resultValidate.user.Id, resultValidate.user);
            return Ok("Ok");
        }
        [HttpPost("SetAlbum")]
        public async Task<ActionResult> SetAlbum(GalleryRequest request)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            if (string.IsNullOrEmpty(request.PhotoId) || request.AlbumId == null) return BadRequest("Wrong data");
            var result = await GalleryService.GetPhotoAsync(resultValidate.user.Id, request.PhotoId);
            if (result == null) return NotFound("Photo not found");
            result.AlbumId = request.AlbumId;
            await GalleryService.UpdatePhotoAsync(resultValidate.user.Id, request.PhotoId, result);
            return Ok("Ok");
        }
        [HttpDelete("RemovePhoto")]
        public async Task<ActionResult> RemovePhoto(string id)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            await GalleryService.RemovePhotoAsync(resultValidate.user.Id, id);
            return Ok("Ok");
        }
        [HttpGet("GetAlbums")]
        public async Task<ActionResult> GetAlbums(string userId)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await GalleryService.GetAlbumsAsync(userId);
            return Ok(result);
        }
        [HttpPost("AddAlbum")]
        public async Task<ActionResult> AddAlbum([FromForm] GalleryRequest request)
        {
            if (string.IsNullOrEmpty(request.Name)) return BadRequest("Name is null or empty");

            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var album = await GalleryService.AddAlbumAsync(resultValidate.user.Id);

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
                        await GalleryService.UpdatePhotoAsync(resultValidate.user.Id, id, photo);
                    }
                }
            }

            album.Object.Id = album.Key;
            album.Object.Name = request.Name;
            await GalleryService.UpdateAlbumAsync(resultValidate.user.Id, album.Key, album.Object);
            return Ok("Ok");
        }
        [HttpDelete("RemoveAlbum")]
        public async Task<ActionResult> RemoveAlbum(string id)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            await GalleryService.RemoveAlbumAsync(resultValidate.user.Id, id);
            return Ok("Ok");
        }
        [HttpDelete("RemoveAlbumAndPhotos")]
        public async Task<ActionResult> RemoveAlbumAndPhotos(string id)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await GalleryService.GetPhotosAsync(resultValidate.user.Id, id);
            if(result == null || result.Count() == 0) return NotFound("Photos not found");
            foreach (var photo in result)
            {
                await GalleryService.RemovePhotoAsync(resultValidate.user.Id, photo.Id);
            }
            await GalleryService.RemoveAlbumAsync(resultValidate.user.Id, id);
            return Ok("Ok");
        }
    }
}
