using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : Controller
    {
        [HttpPost("GetNotifications")]
        public async Task<ActionResult> GetNotifications()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await NotificationService.GetNotificationsAsync(resultValidate.user.Id);

            return Ok(result);
        }
    }
}
