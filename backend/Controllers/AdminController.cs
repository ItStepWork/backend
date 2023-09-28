using backend.Models;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : Controller
    {
        [HttpGet("GetUsers")]
        public async Task<ActionResult> GetUsers()
        {
            var result = await UserService.GetUsersAsync();
            var sortModerator = result?.OrderByDescending(user => user.Role == Role.Moderator);
            var sortAdmin = sortModerator?.OrderByDescending(user=>user.Role == Role.Admin);
            return Ok(sortAdmin);
        }
        [HttpGet("GetGroups")]
        public async Task<ActionResult> GetGroups()
        {
            var result = await GroupService.GetGroupsAsync();
            return Ok(result);
        }
        [HttpGet("GetAllActivity")]
        public async Task<ActionResult> GetAllActivity()
        {
            var result = await ActivityService.GetAllActivityAsync();
            return Ok(result);
        }
        [HttpGet("GetPagesActivity")]
        public async Task<ActionResult> GetPagesActivity(Chart chart)
        {
            var result = await AdminService.GetPagesActivityAsync(chart);
            return Ok(result);
        }
        [HttpGet("GetUsersActivity")]
        public async Task<ActionResult> GetUsersActivity(Chart chart)
        {
            var result = await AdminService.GetUsersActivityAsync(chart);
            return Ok(result);
        }
        [HttpPost("UpdateUserStatus")]
        public async Task<ActionResult> UpdateUserStatus(Request request)
        {
            if (request.Status == null || string.IsNullOrEmpty(request.UserId)) return BadRequest("Data is null or empty");

            await UserService.UpdateUserStatusAsync(request.UserId, (Status)request.Status);
            return Ok("Ok");
        }
        [HttpPost("UpdateUserRole")]
        public async Task<ActionResult> UpdateUserRole(Request request)
        {
            if (request.Role == null || string.IsNullOrEmpty(request.UserId)) return BadRequest("Data is null or empty");

            await UserService.UpdateUserRoleAsync(request.UserId, (Role)request.Role);
            return Ok("Ok");
        }
        [HttpPost("UpdateUserBlockingTime")]
        public async Task<ActionResult> UpdateUserBlockingTime(Request request)
        {
            if (string.IsNullOrEmpty(request.BlockingTime) || string.IsNullOrEmpty(request.UserId)) return BadRequest("Data is null or empty");

            await UserService.UpdateUserBlockingTimeAsync(request.UserId, DateTime.Parse(request.BlockingTime).ToUniversalTime());
            return Ok("Ok");
        }
        [HttpPost("UpdateGroupStatus")]
        public async Task<ActionResult> UpdateGroupStatus(Request request)
        {
            if (request.Status == null || string.IsNullOrEmpty(request.GroupId)) return BadRequest("Data is null or empty");

            await AdminService.UpdateGroupStatusAsync(request.GroupId, (Status)request.Status);
            return Ok("Ok");
        }
        [HttpPost("UpdateGroupBlockingTime")]
        public async Task<ActionResult> UpdateGroupBlockingTime(Request request)
        {
            if (string.IsNullOrEmpty(request.BlockingTime) || string.IsNullOrEmpty(request.GroupId)) return BadRequest("Data is null or empty");

            await AdminService.UpdateGroupBlockingTimeAsync(request.GroupId, DateTime.Parse(request.BlockingTime).ToUniversalTime());
            return Ok("Ok");
        }
        [HttpGet("GetSupportDialogs")]
        public async Task<ActionResult> GetSupportDialogs()
        {
            var result = await AdminService.GetSupportDialogsAsync();
            return Ok(result);
        }
        [HttpGet("GetSupportMessages")]
        public async Task<ActionResult> GetSupportMessages(string id)
        {
            var result = await SupportService.GetMessagesAsync(id);
            return Ok(result);
        }
        [HttpPost("SendSupportMessage")]
        public async Task<ActionResult> SendSupportMessage([FromForm] Request request)
        {
            if (string.IsNullOrEmpty(request.Text) || string.IsNullOrEmpty(request.UserId)) return BadRequest("Data is null or empty");

            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            await AdminService.SendSupportMessageAsync(userId, request);
            return Ok("Ok");
        }
    }
}
