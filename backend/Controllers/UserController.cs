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
        public async Task<ActionResult> AddFriend(string id)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            var result = await UserService.FindFriendAsync(resultValidate.user.Id, id);

            if (result != null) return Conflict("Invitation already sent");

            bool check = await UserService.AddFriendAsync(resultValidate.user.Id, id);
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
        [HttpPost("AddGroup")]
        public async Task<ActionResult> AddGroup(Group group)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            group.AdminId = resultValidate.user.Id;
            var result = await UserService.AddGroupAsync(group);
            if (result.Object == null) return Conflict("Error");
            group.Id = result.Key;
            group.Users.Add(resultValidate.user.Id,true);
            await UserService.UpdateGroupAsync(result.Key, group);
            return Ok("Group added");
        }
        [Authorize]
        [HttpGet("GetGroups")]
        public async Task<ActionResult> GetGroups()
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            IEnumerable<Group>? groups = await UserService.GetGroupsAsync();
            return Ok(groups);
        }
        [Authorize]
        [HttpPost("JoinGroup")]
        public async Task<ActionResult> JoinGroup(string id)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            Group? group = await UserService.GetGroupByIdAsync(id);
            if (group == null) return NotFound("Group Not Found!");
            group.Users.Add(resultValidate.user.Id,group.Audience == Audience.Private? false:true);
            //await Console.Out.WriteLineAsync(group);
            await UserService.UpdateGroupAsync(id, group);
            return Ok("Request has been sent");
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
        [HttpPost("SendMessage")]
        public async Task<ActionResult> SendMessage([FromForm]MessageData data)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            User? recipient = await UserService.FindUserByIdAsync(data.Id);
            if (recipient == null) return NotFound("Recipient not found!");

            Message? message = await UserService.SendMessageAsync(resultValidate.user.Id, data);
            if (message == null || message.Id == null) return Conflict("Send message failed");

            return Ok("Ok");
        }
        [Authorize]
        [HttpGet("GetDialogs")]
        public async Task<ActionResult> GetDialogs()
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await UserService.GetDialogs(resultValidate.user.Id);
            return Ok(result);
        }
        [Authorize]
        [HttpDelete("RemoveDialog")]
        public async Task<ActionResult> RemoveDialog(string id)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            await UserService.RemoveDialogAsync(resultValidate.user.Id, id);
            return Ok("Ok");
        }
        [Authorize]
        [HttpGet("GetMessages")]
        public async Task<ActionResult> GetMessages(string id)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            var result = await UserService.GetMessages(resultValidate.user.Id, id);
            return Ok(result);
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
