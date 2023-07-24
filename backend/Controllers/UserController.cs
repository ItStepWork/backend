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
        [HttpGet("GetUsers")]
        public async Task<ActionResult> GetUsers()
        {
            IEnumerable<UserBase>? users = await UserService.GetUsersAsync();
            return Ok(users);
        }
        [Authorize]
        [HttpGet("GetFriends")]
        public async Task<ActionResult> GetFriends()
        {
            Claim? claimId = this.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid);
            if (claimId == null) return NotFound("User not authorize!");

            User? sender = await UserService.FindUserByIdAsync(claimId.Value);
            if (sender == null) return NotFound("Sender not found!");

            var result = await UserService.GetFriendsAsync(claimId.Value);

            return Ok(result);
        }
        [Authorize]
        [HttpPost("SendMessage")]
        public async Task<ActionResult> SendMessage(string id, string text)
        {
            Claim? claimId = this.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid);
            if (claimId == null) return NotFound("User not authorize!");

            User? sender = await UserService.FindUserByIdAsync(claimId.Value);
            if (sender == null) return NotFound("Sender not found!");

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            Message? message = await UserService.SendMessageAsync(claimId.Value, id, text);
            if (message == null) return Conflict("Send message failed");

            return Ok("Ok");
        }
        [Authorize]
        [HttpPost("AddFriend")]
        public async Task<ActionResult> AddFriend(string id)
        {
            Claim? claimId = this.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid);
            if (claimId == null) return NotFound("User not authorize!");

            User? sender = await UserService.FindUserByIdAsync(claimId.Value);
            if (sender == null) return NotFound("Sender not found!");

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            var result = await UserService.FindFriendAsync(claimId.Value, id);

            if (result != null) return Conflict("Invitation already sent");

            bool check = await UserService.AddFriendAsync(claimId.Value, id);
            if (!check) return Conflict("Add friend failed");
            else return Ok("Friend invite sent");
        }
        [Authorize]
        [HttpPost("ConfirmFriend")]
        public async Task<ActionResult> ConfirmFriend(string id)
        {
            Claim? claimId = this.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid);
            if (claimId == null) return NotFound("User not authorize!");

            User? sender = await UserService.FindUserByIdAsync(claimId.Value);
            if (sender == null) return NotFound("Sender not found!");

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            var result = await UserService.ConfirmFriendAsync(claimId.Value, id);

            if (!result) return Conflict("Confirm friend failed");
            else return Ok("Friend added");
        }
        [Authorize]
        [HttpDelete("RemoveFriend")]
        public async Task<ActionResult> RemoveFriend(string id)
        {
            Claim? claimId = this.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid);
            if (claimId == null) return NotFound("User not authorize!");

            User? sender = await UserService.FindUserByIdAsync(claimId.Value);
            if (sender == null) return NotFound("Sender not found!");

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            await UserService.RemoveFriendAsync(claimId.Value, id);

            return Ok("Friend removed");
        }
    }
}
