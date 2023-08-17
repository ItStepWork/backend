using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GroupController : Controller
    {
        [Authorize]
        [HttpPost("AddGroup")]
        public async Task<ActionResult> AddGroup([FromForm] GroupRequest groupRequest)
        {
            (string response, User? user) resultValidate = await ValidationUser();
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
        [Authorize]
        [HttpGet("GetGroups")]
        public async Task<ActionResult> GetGroups()
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);
            IEnumerable<Group>? groups = await GroupService.GetGroupsAsync();
            var sort = groups.ToList();
            sort.Sort((y, x) => Convert.ToInt32(x.AdminId.Equals(resultValidate.user.Id)) - Convert.ToInt32(y.AdminId.Equals(resultValidate.user.Id)));
            sort.Sort((y, x) => Convert.ToInt32(x.Users.ContainsKey(resultValidate.user.Id)) - Convert.ToInt32(y.Users.ContainsKey(resultValidate.user.Id)));
            return Ok(sort);
        }
        [Authorize]
        [HttpGet("GetGroup")]
        public async Task<ActionResult> GetGroup(string id)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);
            var group = await GroupService.GetGroupAsync(id);
            return Ok(group);
        }
        [Authorize]
        [HttpPost("JoinGroup")]
        public async Task<ActionResult> JoinGroup(GroupRequest groupRequest)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            Group? group = await GroupService.GetGroupByIdAsync(groupRequest.Id);
            if (group == null) return NotFound("Group Not Found!");
            group.Users[resultValidate.user.Id] = group.Audience == Audience.Private ? false : true;
            await GroupService.UpdateGroupAsync(groupRequest.Id, group);
            return Ok("Request has been sent");
        }
        [Authorize]
        [HttpDelete("LeaveGroup")]
        public async Task<ActionResult> LeaveGroup(string id)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);
            await GroupService.RemuveUserFromGroupAsync(id, resultValidate.user.Id);
            return Ok("You leave the group");
        }
        [Authorize]
        [HttpGet("GetUsersGroup")]
        public async Task<ActionResult> GetUsersGroup(string id)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);
            var group = await GroupService.GetGroupAsync(id);
            var users = group.Users.Select((a) => a.Key);
            var result = await UserService.GetUsersAsync(users);
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
