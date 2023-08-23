using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FriendController : Controller
    {
        [HttpGet("GetFriends")]
        public async Task<ActionResult> GetFriends()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await FriendService.GetFriendsAsync(resultValidate.user.Id);

            return Ok(result);
        }
        [HttpPost("AddFriend")]
        public async Task<ActionResult> AddFriend(FriendRequest request)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            User? recipient = await UserService.FindUserByIdAsync(request.UserId);
            if (recipient == null) return NotFound("Recipient not found!");

            var result = await FriendService.FindFriendAsync(resultValidate.user.Id, request.UserId);

            if (result != null) return Conflict("Invitation already sent");

            bool check = await FriendService.AddFriendAsync(resultValidate.user.Id, request.UserId);
            if (!check) return Conflict("Add friend failed");
            else return Ok("Friend invite sent");
        }
        [HttpPost("ConfirmFriend")]
        public async Task<ActionResult> ConfirmFriend(string id)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            var result = await FriendService.ConfirmFriendAsync(resultValidate.user.Id, id);

            if (!result) return Conflict("Confirm friend failed");
            else return Ok("Friend added");
        }
        [HttpDelete("RemoveFriend")]
        public async Task<ActionResult> RemoveFriend(string id)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            await FriendService.RemoveFriendAsync(resultValidate.user.Id, id);

            return Ok("Friend removed");
        }
    }
}
