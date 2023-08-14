using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GalleryController : Controller
    {
        [Authorize]
        [HttpGet("GetPhotos")]
        public async Task<ActionResult> GetPhotos()
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var photos = await GalleryService.GetPhotosAsync(resultValidate.user.Id);
            return Ok(photos);
        }
        [Authorize]
        [HttpPost("AddPhoto")]
        public async Task<ActionResult> AddPhoto(IFormFile file)
        {
            (string response, User? user) resultValidate = await ValidationUser();
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
        [Authorize]
        [HttpPost("SendCommentPhoto")]
        public async Task<ActionResult> SendCommentPhoto(GalleryRequest request)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            await GalleryService.SendCommentPhotoAsync(resultValidate.user.Id, request.UserId, request.PhotoId, request.Text);
            return Ok("Ok");
        }
        [Authorize]
        [HttpPost("SetLikePhoto")]
        public async Task<ActionResult> SetLikePhoto(GalleryRequest request)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            await GalleryService.SetLikePhotoAsync(resultValidate.user.Id, request.UserId, request.PhotoId);
            return Ok("Ok");
        }
        [Authorize]
        [HttpPost("SetAvatar")]
        public async Task<ActionResult> SetAvatar(GalleryRequest request)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            resultValidate.user.AvatarUrl = request.Url;
            await UserService.UpdateUserAsync(resultValidate.user.Id, resultValidate.user);
            return Ok("Ok");
        }
        [Authorize]
        [HttpPost("SetBackground")]
        public async Task<ActionResult> SetBackground(GalleryRequest request)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            resultValidate.user.BackgroundUrl = request.Url;
            await UserService.UpdateUserAsync(resultValidate.user.Id, resultValidate.user);
            return Ok("Ok");
        }
        [Authorize]
        [HttpDelete("RemovePhoto")]
        public async Task<ActionResult> RemovePhoto(string id)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            await GalleryService.RemovePhotoAsync(resultValidate.user.Id, id);
            return Ok("Ok");
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
