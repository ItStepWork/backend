using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        [Authorize]
        [HttpGet("GetUser")]
        public async Task<ActionResult> GetUser(string id)
        {
            
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);
            
            var user = await UserService.GetUserAsync(id);
            return Ok(user);
        }
        [Authorize]
        [HttpGet("GetUsers")]
        public async Task<ActionResult> GetUsers()
        {

            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            IEnumerable<UserBase>? users = await UserService.GetUsersAsync(resultValidate.user.Id);
            return Ok(users);
        }
        [Authorize]
        [HttpGet("GetFriends")]
        public async Task<ActionResult> GetFriends()
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await UserService.GetFriendsAsync(resultValidate.user.Id);

            return Ok(result);
        }
        [Authorize]
        [HttpPost("AddFriend")]
        public async Task<ActionResult> AddFriend(FriendsRequest request)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            User? recipient = await UserService.FindUserByIdAsync(request.UserId);
            if (recipient == null) return NotFound("Recipient not found!");

            var result = await UserService.FindFriendAsync(resultValidate.user.Id, request.UserId);

            if (result != null) return Conflict("Invitation already sent");

            bool check = await UserService.AddFriendAsync(resultValidate.user.Id, request.UserId);
            if (!check) return Conflict("Add friend failed");
            else return Ok("Friend invite sent");
        }
        [Authorize]
        [HttpPost("ConfirmFriend")]
        public async Task<ActionResult> ConfirmFriend(string id)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            var result = await UserService.ConfirmFriendAsync(resultValidate.user.Id, id);

            if (!result) return Conflict("Confirm friend failed");
            else return Ok("Friend added");
        }
        [Authorize]
        [HttpDelete("RemoveFriend")]
        public async Task<ActionResult> RemoveFriend(string id)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            await UserService.RemoveFriendAsync(resultValidate.user.Id, id);

            return Ok("Friend removed");
        }

        [Authorize]
        [HttpPost("UpdateUser")]
        public async Task<ActionResult> UpdateUser(User data)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            User user = resultValidate.user;

            // Check email
            if(string.IsNullOrEmpty(data.Email)) return BadRequest("Email null or empty");
            data.Email = data.Email.ToLower();

            if(user.Email != data.Email)
            {
                var addr = new System.Net.Mail.MailAddress(data.Email);
                if (addr.Address != data.Email) return BadRequest("Email not validate");
                var find = await UserService.FindUserByEmailAsync(data.Email);
                if (find != null) return Conflict("Email exist"); 
            }

            // Update user
            if(!string.IsNullOrEmpty(data.Born))
            {
                DateOnly born = DateOnly.Parse(data.Born.ToString());
                user.Born = born.ToLongDateString();
            }
            user.Email = data.Email;
            user.FirstName = data.FirstName;
            user.LastName = data.LastName;
            user.Phone = data.Phone;
            user.Gender = data.Gender;
            user.Status = Status.Active;
            user.FamilyStatus = data.FamilyStatus;
            user.AboutMe = data.AboutMe;
            user.Location = data.Location;
            user.Work = data.Work;
            await UserService.UpdateUserAsync(resultValidate.user.Id, user);
            return Ok("User is Updated");
        }
        [Authorize]
        [HttpPost("SaveAvatar")]
        public async Task<ActionResult> SaveAvatar(IFormFile file)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);


            var photo = await GalleryService.AddPhotoAsync(resultValidate.user.Id);

            var url = await UserService.SaveFileAsync(file, "Photos", photo.Key);
            if (url == null) return Conflict("Save avatar failed");

            Photo result = photo.Object;
            result.Id = photo.Key;
            result.Url = url;

            await GalleryService.UpdatePhotoAsync(resultValidate.user.Id, photo.Key, result);

            resultValidate.user.AvatarUrl = url;

            await UserService.UpdateUserAsync(resultValidate.user.Id, resultValidate.user);

            return Ok(resultValidate.user);
        }
        [Authorize]
        [HttpPost("SaveBackground")]
        public async Task<ActionResult> SaveBackground(IFormFile file)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var photo = await GalleryService.AddPhotoAsync(resultValidate.user.Id);

            var url = await UserService.SaveFileAsync(file, "Photos", photo.Key);
            if (url == null) return Conflict("Save background failed");

            Photo result = photo.Object;
            result.Id = photo.Key;
            result.Url = url;

            await GalleryService.UpdatePhotoAsync(resultValidate.user.Id, photo.Key, result);

            resultValidate.user.BackgroundUrl = url;

            await UserService.UpdateUserAsync(resultValidate.user.Id, resultValidate.user);

            return Ok(resultValidate.user);
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
