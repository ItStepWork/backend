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
        public async Task<ActionResult> GetFriends(string id)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var result = await FriendService.GetFriendsAsync(userId, id);
            return Ok(result);
        }
        [HttpGet("GetFriend")]
        public async Task<ActionResult> GetFriend(string id)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var result = await FriendService.GetFriendAsync(userId, id);
            return Ok(result);
        }
        [HttpGet("GetFriendsCount")]
        public async Task<ActionResult> GetFriendsCount(string id)
        {
            var result = await FriendService.GetFriendsCount(id);
            return Ok(result);
        }
        [HttpGet("GetConfirmedFriends")]
        public async Task<ActionResult> GetConfirmedFriends(string id)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var result = await FriendService.GetConfirmedFriends(userId, id);
            return Ok(result);
        }
        [HttpGet("GetUnconfirmedFriends")]
        public async Task<ActionResult> GetUnconfirmedFriends(string id)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var result = await FriendService.GetUnconfirmedFriends(userId, id);
            return Ok(result);
        }
        [HttpGet("GetWaitingFriends")]
        public async Task<ActionResult> GetWaitingFriends(string id)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var result = await FriendService.GetWaitingFriends(userId, id);
            return Ok(result);
        }
        [HttpGet("GetOtherUsers")]
        public async Task<ActionResult> GetOtherUsers(string id)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var result = await FriendService.GetOtherUsers(userId, id);
            return Ok(result);
        }
        [HttpPost("AddFriend")]
        public async Task<ActionResult> AddFriend(Request request)
        {
            if (string.IsNullOrEmpty(request.Id)) return BadRequest("Data is null or empty");
            var user = HttpContext.Items["user"] as User;
            if (user == null || user.Id == null) return Conflict("User is null");
            if (user.Id == request.Id) return Conflict("Trying to add myself");

            User? recipient = await UserService.FindUserByIdAsync(request.Id);
            if (recipient == null) return NotFound("Recipient not found!");

            var result = await FriendService.FindFriendAsync(user.Id, request.Id);

            if (result != null) return Conflict("Invitation already sent");

            bool check = await FriendService.AddFriendAsync(user.Id, request.Id);
            if (!check) return Conflict("Add friend failed");
            else return Ok("Friend invite sent");
        }
        [HttpPost("ConfirmFriend")]
        public async Task<ActionResult> ConfirmFriend(Request request)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            User? recipient = await UserService.FindUserByIdAsync(request.Id);
            if (recipient == null) return NotFound("Recipient not found!");

            var result = await FriendService.ConfirmFriendAsync(userId, request.Id);

            if (!result) return Conflict("Confirm friend failed");
            else return Ok("Friend added");
        }
        [HttpDelete("RemoveFriend")]
        public async Task<ActionResult> RemoveFriend(string id)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            await FriendService.RemoveFriendAsync(userId, id);
            return Ok("Friend removed");
        }
    }
}
