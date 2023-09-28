using backend.Services;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using backend.Models.Enums;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : Controller
    {
        [HttpGet("GetNotifications")]
        public async Task<ActionResult> GetNotifications()
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var result = await NotificationService.GetNotificationsAsync(userId);
            return Ok(result);
        }
        [HttpPost("InviteToGroup")]
        public async Task<ActionResult> InviteToGroup(Request request)
        {
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.GroupId)) return BadRequest("Data in null or empty");
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            Group? group = await GroupService.GetGroupAsync(request.GroupId);
            if (group == null) return NotFound("Group not found");

            if (group.Users.ContainsKey(request.UserId)) return Conflict("User exists in the group");

            await NotificationService.AddNotificationAsync(userId, request.UserId, group, NotificationType.InviteToGroup);

            return Ok("Ok");
        }
    }
}
