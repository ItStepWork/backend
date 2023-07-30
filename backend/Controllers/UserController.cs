using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
            (string response, string userId) resultValidate = await ValidationUser();
            if (resultValidate.response != "") return NotFound(resultValidate.response);

            IEnumerable<UserBase>? users = await UserService.GetUsersAsync(resultValidate.userId);
            return Ok(users);
        }
        [Authorize]
        [HttpGet("GetFriends")]
        public async Task<ActionResult> GetFriends()
        {
            (string response, string userId) resultValidate = await ValidationUser();
            if (resultValidate.response != "") return NotFound(resultValidate.response);

            var result = await UserService.GetFriendsAsync(resultValidate.userId);

            return Ok(result);
        }
        [Authorize]
        [HttpPost("SendMessage")]
        public async Task<ActionResult> SendMessage(string id, string text)
        {
            (string response, string userId) resultValidate = await ValidationUser();
            if (resultValidate.response != "") return NotFound(resultValidate.response);

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            Message? message = await UserService.SendMessageAsync(resultValidate.userId, id, text);
            if (message == null) return Conflict("Send message failed");

            return Ok("Ok");
        }
        [Authorize]
        [HttpPost("AddFriend")]
        public async Task<ActionResult> AddFriend(string id)
        {
            (string response, string userId) resultValidate = await ValidationUser();
            if (resultValidate.response != "") return NotFound(resultValidate.response);

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            var result = await UserService.FindFriendAsync(resultValidate.userId, id);

            if (result != null) return Conflict("Invitation already sent");

            bool check = await UserService.AddFriendAsync(resultValidate.userId, id);
            if (!check) return Conflict("Add friend failed");
            else return Ok("Friend invite sent");
        }
        [Authorize]
        [HttpPost("ConfirmFriend")]
        public async Task<ActionResult> ConfirmFriend(string id)
        {
            (string response, string userId) resultValidate = await ValidationUser();
            if (resultValidate.response != "") return NotFound(resultValidate.response);

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            var result = await UserService.ConfirmFriendAsync(resultValidate.userId, id);

            if (!result) return Conflict("Confirm friend failed");
            else return Ok("Friend added");
        }
        [Authorize]
        [HttpDelete("RemoveFriend")]
        public async Task<ActionResult> RemoveFriend(string id)
        {
            (string response, string userId) resultValidate = await ValidationUser();
            if (resultValidate.response != "") return NotFound(resultValidate.response);

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            await UserService.RemoveFriendAsync(resultValidate.userId, id);

            return Ok("Friend removed");
        }
        [Authorize]
        [HttpPost("AddGroup")]
        public async Task<ActionResult> AddGroup(string name)
        {
            (string response, string userId) resultValidate = await ValidationUser();
            if (resultValidate.response != "") return NotFound(resultValidate.response);

            Group group = new Group() { AdminId = resultValidate.userId, Name = name };
            var result = await UserService.AddGroupAsync(group);
            if (result.Object == null) return Conflict("Error");
            group.Id = result.Key;
            await UserService.UpdateGroupAsync(result.Key, group);
            return Ok("Group added");
        }

        [Authorize]
        [HttpPost("UpdateUser")]
        public async Task<ActionResult> UpdateUser(User data)
        {
            Console.WriteLine(JsonConvert.SerializeObject(data));

            (string response, string userId) resultValidate = await ValidationUser();
            if (resultValidate.response != "") return NotFound(resultValidate.response);
            
            var user = await UserService.FindUserByIdAsync(resultValidate.userId);

            if (user != null)
            {
                // Update user
                DateOnly born = DateOnly.Parse(data.Born.ToString());
                user.Born = born.ToLongDateString();       
                user.FirstName = data.FirstName;
                user.LastName = data.LastName;
                user.Phone = data.Phone;
                user.Gender = data.Gender;
                user.Status = Status.Active;                                                                        
                user.FamilyStatus = data.FamilyStatus;
                user.AboutMe = data.AboutMe;
            
            }
            
            await UserService.UpdateUserAsync(resultValidate.userId, user);
            Console.WriteLine(JsonConvert.SerializeObject(user));
            return Ok("User is Updated");
        }
        
       
        private async Task<(string, string)> ValidationUser()
        {
            Claim? claimId = this.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid);
            if (claimId == null) return ("User not authorize!", "");

            User? sender = await UserService.FindUserByIdAsync(claimId.Value);
            if (sender == null) return ("Sender not found!", "");

            sender.LastVisit = DateTime.UtcNow;
            await UserService.UpdateUserAsync(claimId.Value, sender);
            return ("", claimId.Value);
        }
    }
}
