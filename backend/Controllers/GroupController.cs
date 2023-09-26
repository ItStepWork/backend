using backend.Models;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GroupController : Controller
    {
        [HttpPost("AddGroup")]
        public async Task<ActionResult> AddGroup([FromForm] Request groupRequest)
        {
            if (groupRequest.Audience == null|| groupRequest.File == null) return BadRequest("Audience or File is null");
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            var result = await GroupService.AddGroupAsync(groupRequest, resultValidate.user.Id);
            if(result.ok == "")return Conflict(result.response);
            return Ok(result.ok);
        }
        [HttpDelete("DeleteGroup")]
        public async Task<ActionResult> DeleteGroup(string id)
        {
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            var group = await GroupService.GetGroupAsync(id);
            if (group == null) return NotFound("Group not found");
            if (group.AdminId != resultValidate.user.Id) return Conflict("You not Admin");

            await GroupService.RemuveGroupAsync(id);
            return Ok("Group deleted");
        }
        [HttpPost("UpdateAvatar")]
        public async Task<ActionResult> UpdateAvatar([FromForm] Request groupRequest)
        {
            if (groupRequest.Id == null || groupRequest.File == null) return BadRequest("Audience or File is null");
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            var group = await GroupService.GetGroupAsync(groupRequest.Id);
            if (group == null) return NotFound("Group not found");
            if (group.AdminId != resultValidate.user.Id) return Conflict("You not Admin");
            var url = await UserService.SaveFileAsync(groupRequest.File, "Groups", group.Id);
            group.PictureUrl = url;
            await GroupService.UpdateGroupAsync(group);
            return Ok("Avatar updated");
        }
        [HttpPost("UpdateGroup")]
        public async Task<ActionResult> UpdateGroup([FromForm] Request groupRequest)
        {
            if (groupRequest.Audience == null || groupRequest.Id == null || groupRequest.Email==null) return BadRequest("Audience or Id or Email is null");
            var addr = new System.Net.Mail.MailAddress(groupRequest.Email);
            if (addr.Address != groupRequest.Email) return BadRequest("Email not validate");
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            var group = await GroupService.GetGroupAsync(groupRequest.Id);
            if (group == null) return NotFound("Group not found");
            if (group.AdminId != resultValidate.user.Id) return Conflict("You not Admin");

            group.Description = groupRequest.Description;
            group.Audience = (Audience)groupRequest.Audience;
            group.Name = groupRequest.Name;
            group.Email = groupRequest.Email;
            await GroupService.UpdateGroupAsync(group);
            return Ok("Group updated");
        }
        [HttpGet("GetGroups")]
        public async Task<ActionResult> GetGroups(string userId)
        {
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            IEnumerable<Group>? groups = await GroupService.GetGroupsAsync();
            if (groups == null) return NotFound("Groups not found");

            List<Group> sort;
            if (resultValidate.user.Id != userId) sort = groups.Where((i) => i.Users.ContainsKey(userId) && i.Users[userId]).ToList();
            else sort = groups.ToList();
            sort.Sort((y, x) => Convert.ToInt32(x.AdminId.Equals(resultValidate.user.Id)) - Convert.ToInt32(y.AdminId.Equals(resultValidate.user.Id)));
            sort.Sort((y, x) => Convert.ToInt32(x.Users.ContainsKey(resultValidate.user.Id)) - Convert.ToInt32(y.Users.ContainsKey(resultValidate.user.Id)));
            return Ok(sort);
        }
        [HttpGet("GetGroup")]
        public async Task<ActionResult> GetGroup(string id)
        {
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            var group = await GroupService.GetGroupAsync(id);
            if (group == null) return NotFound("Group not found");
            return Ok(group);
        }
        [HttpPost("JoinGroup")]
        public async Task<ActionResult> JoinGroup(Request groupRequest)
        {
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            Group? group = await GroupService.GetGroupAsync(groupRequest.Id);
            if (group == null) return NotFound("Group Not Found!");

            group.Users[resultValidate.user.Id] = group.Audience == Audience.Private ? false : true;
            await GroupService.UpdateGroupAsync(group);
            return Ok("Request has been sent");
        }
        [HttpDelete("LeaveGroup")]
        public async Task<ActionResult> LeaveGroup(string id)
        {
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            await GroupService.RemuveUserFromGroupAsync(id, resultValidate.user.Id);
            return Ok("You leave the group");
        }
        [HttpPost("RemoveUserFromGroup")]
        public async Task<ActionResult> RemoveUserFromGroup(Request groupRequest)
        {
            if (groupRequest.Id == null || groupRequest.UserId == null) return BadRequest("Id or UserId is null");
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            var group = await GroupService.GetGroupAsync(groupRequest.Id);
            if (group == null) return NotFound("Group not found");
            if (group.AdminId != resultValidate.user.Id) return Conflict("You not Admin"); 

            await GroupService.RemuveUserFromGroupAsync(groupRequest.Id, groupRequest.UserId);
            return Ok("Removed");
        }
        [HttpPost("AcceptUserToGroup")]
        public async Task<ActionResult> AcceptUserToGroup(Request groupRequest)
        {
            if (string.IsNullOrEmpty(groupRequest.UserId)|| string.IsNullOrEmpty(groupRequest.Id)) return BadRequest("GroupRequest is null or empty");
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            var group = await GroupService.GetGroupAsync(groupRequest.Id);
            if(group == null|| group.Users == null) return NotFound("Group or Users is null");
            group.Users[groupRequest.UserId] = true;
            await GroupService.UpdateGroupAsync(group);
            return Ok("Accepted");
        }
        [HttpGet("GetUsersGroup")]
        public async Task<ActionResult> GetUsersGroup(string id)
        {
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            var group = await GroupService.GetGroupAsync(id);
            var users = group?.Users?.Select((a) => a.Key);
            var result = await UserService.GetUsersAsync(users.ToArray());
            result?.Sort((left, right) => left.FirstName == right.FirstName ? left.LastName.CompareTo(right.LastName) : left.FirstName.CompareTo(right.FirstName));
            result?.Sort((y, x) => Convert.ToInt32(x?.Id?.Equals(group?.AdminId)) - Convert.ToInt32(y?.Id?.Equals(group?.AdminId)));
            return Ok(result);
        }
        [HttpGet("GetFriendsInGroup")]
        public async Task<ActionResult> GetFriendsInGroup(string id)
        {
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            var result = await GroupService.GetFriendsInGroup(id, resultValidate.user.Id);
            if (result.friends == null) return NotFound(result.response);
                return Ok(result.friends);
        }
        [HttpGet("GetFriendsForInvitation")]
        public async Task<ActionResult> GetFriendsForInvitation(string id)
        {
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            var result = await GroupService.GetFriendsForInvitation(id, resultValidate.user.Id);
            if (result.friends == null) return NotFound(result.response);
            return Ok(result.friends);
        }
        [HttpGet("GetMembersGroup")]
        public async Task<ActionResult> GetMembersGroup(string id)
        {
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            var result = await GroupService.GetMembersGroup(id);
            if (result.users == null) return NotFound(result.response);
            return Ok(result.users);
        }
        [HttpGet("GetRequestsToGroup")]
        public async Task<ActionResult> GetRequestsToGroup(string id)
        {
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            var result = await GroupService.GetRequestsToGroup(id);
            if (result.users == null) return NotFound(result.response);
            return Ok(result.users);
        }
        [HttpPost("AddPhoto")]
        public async Task<ActionResult> AddPhoto([FromForm] Request groupRequest)
        {
            if (groupRequest.Id == null || groupRequest.File == null) return BadRequest("Id or Photo is null");
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            var result = await GroupService.AddPhoto(groupRequest, resultValidate.user.Id);
            if (result.ok == "") return NotFound(result.response);
            return Ok(result.ok);
        }
        [HttpGet("GetPhotos")]
        public async Task<ActionResult> GetPhotos(string groupId)
        {
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            var photos = await GalleryService.GetPhotosAsync(groupId);
            return Ok(photos);
        }
        [HttpPost("RemovePhoto")]
        public async Task<ActionResult> RemovePhoto(Request groupRequest)
        {
            if (groupRequest.Id == null || groupRequest.PhotoId == null) return BadRequest("Id or Photo is null");
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            var result = await GroupService.RemovePhoto(groupRequest, resultValidate.user.Id);
            if (result.ok == "") return NotFound(result.response);
            return Ok(result.ok);
        }
    }
}
