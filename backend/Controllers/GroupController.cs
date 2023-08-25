using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GroupController : Controller
    {
        [HttpPost("AddGroup")]
        public async Task<ActionResult> AddGroup([FromForm] GroupRequest groupRequest)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            Group group = new Group();
            group.Name = groupRequest.Name;
            group.Description = groupRequest.Description;
            group.Audience = groupRequest.Audience;
            group.AdminId= resultValidate.user.Id;
            group.Users.Add(resultValidate.user.Id, true);
            var result = await GroupService.AddGroupAsync(group);
            if (result.Object == null) return Conflict("Error");
            group.Id = result.Key;
            var url = await UserService.SaveFileAsync(groupRequest.File, "Groups", group.Id);
            group.PictureUrl = url;
            await GroupService.UpdateGroupAsync(result.Key, group);
            return Ok("Group added");
        }
        [HttpPost("UpdateAvatar")]
        public async Task<ActionResult> UpdateAvatar([FromForm] GroupRequest groupRequest)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);
            var group = await GroupService.GetGroupAsync(groupRequest.Id);
            var url = await UserService.SaveFileAsync(groupRequest.File, "Groups", group.Id);
            group.PictureUrl = url;
            await GroupService.UpdateGroupAsync(group.Id, group);
            return Ok("Avatar updated");
        }
        [HttpPost("UpdateGroup")]
        public async Task<ActionResult> UpdateGroup([FromForm] GroupRequest groupRequest)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);
            var group = await GroupService.GetGroupAsync(groupRequest.Id);
            group.Description = groupRequest.Description;
            group.Audience = groupRequest.Audience;
            group.Name = groupRequest.Name;
            await GroupService.UpdateGroupAsync(group.Id, group);
            return Ok("Group updated");
        }
        [HttpGet("GetGroups")]
        public async Task<ActionResult> GetGroups()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            IEnumerable<Group>? groups = await GroupService.GetGroupsAsync();
            var sort = groups.ToList();
            sort.Sort((y, x) => Convert.ToInt32(x.AdminId.Equals(resultValidate.user.Id)) - Convert.ToInt32(y.AdminId.Equals(resultValidate.user.Id)));
            sort.Sort((y, x) => Convert.ToInt32(x.Users.ContainsKey(resultValidate.user.Id)) - Convert.ToInt32(y.Users.ContainsKey(resultValidate.user.Id)));
            return Ok(sort);
        }
        [HttpGet("GetGroup")]
        public async Task<ActionResult> GetGroup(string id)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var group = await GroupService.GetGroupAsync(id);
            return Ok(group);
        }
        [HttpPost("JoinGroup")]
        public async Task<ActionResult> JoinGroup(GroupRequest groupRequest)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            Group? group = await GroupService.GetGroupByIdAsync(groupRequest.Id);
            if (group == null) return NotFound("Group Not Found!");
            group.Users[resultValidate.user.Id] = group.Audience == Audience.Private ? false : true;
            await GroupService.UpdateGroupAsync(groupRequest.Id, group);
            return Ok("Request has been sent");
        }
        [HttpDelete("LeaveGroup")]
        public async Task<ActionResult> LeaveGroup(string id)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            await GroupService.RemuveUserFromGroupAsync(id, resultValidate.user.Id);
            return Ok("You leave the group");
        }
        [HttpPost("RemoveUserFromGroup")]
        public async Task<ActionResult> RemoveUserFromGroup(GroupRequest groupRequest)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            await GroupService.RemuveUserFromGroupAsync(groupRequest.Id, groupRequest.UserId);
            return Ok("Removed");
        }
        [HttpPost("AcceptUserToGroup")]
        public async Task<ActionResult> AcceptUserToGroup(GroupRequest groupRequest)
        {
            if (string.IsNullOrEmpty(groupRequest.UserId)|| string.IsNullOrEmpty(groupRequest.Id)) return BadRequest("GroupRequest is null or empty");
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var group = await GroupService.GetGroupAsync(groupRequest.Id);
            if(group==null|| group.Users==null) return NotFound("Group is null");
            group.Users[groupRequest.UserId] = true;
            await GroupService.UpdateGroupAsync(groupRequest.Id, group);
            return Ok("Accepted");
        }
        [HttpGet("GetUsersGroup")]
        public async Task<ActionResult> GetUsersGroup(string id)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var group = await GroupService.GetGroupAsync(id);
            var users = group?.Users?.Select((a) => a.Key);
            var result = await UserService.GetUsersAsync(users);
            return Ok(result);
        }
        [HttpGet("GetMembersGroup")]
        public async Task<ActionResult> GetMembersGroup(string id)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var group = await GroupService.GetGroupAsync(id);
            var users = group?.Users?.Where((a)=>a.Value==true).Select((a) => a.Key);
            var result = await UserService.GetUsersAsync(users);
            return Ok(result);
        }
        [HttpGet("GetRequestsToGroup")]
        public async Task<ActionResult> GetRequestsToGroup(string id)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var group = await GroupService.GetGroupAsync(id);
            var users = group?.Users?.Where((a) => a.Value == false).Select((a) => a.Key);
            var result = await UserService.GetUsersAsync(users);
            return Ok(result);
        }
    }
}
