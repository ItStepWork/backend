using backend.Services;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : Controller
    {
        [HttpGet("GetNotifications")]
        public async Task<ActionResult> GetNotifications()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await NotificationService.GetNotificationsAsync(resultValidate.user.Id);

            return Ok(result);
        }
        [HttpPost("InviteToGroup")]
        public async Task<ActionResult> InviteToGroup(Request request)
        {
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.GroupId)) return BadRequest("Data in null or empty");
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            Group? group = await GroupService.GetGroupAsync(request.GroupId);
            if (group == null) return NotFound("Group not found");

            if (group.Users.ContainsKey(request.UserId)) return Conflict("User exists in the group");

            await NotificationService.AddNotificationAsync(resultValidate.user.Id, request.UserId, request.GroupId, NotificationType.InviteToGroup);

            return Ok("Ok");
        }
    }
}
