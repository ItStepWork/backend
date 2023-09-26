using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CelebrationController : Controller
    {
        [HttpGet("GetBirthdaysNow")]
        public async Task<ActionResult> GetBirthdaysNow(string id)
        {
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            var result = await CelebrationService.GetBirthdaysEventNowAsync(id);

            return Ok(result);
        }
        [HttpGet("GetBirthdaysSoon")]
        public async Task<ActionResult> GetBirthdaysSoon(string id)
        {
            var resultValidate = await UserService.ValidationUser(this);
            if (resultValidate.user == null || resultValidate.user.Id == null) return resultValidate.response;

            var result = await CelebrationService.GetBirthdaysEventSoonAsync(id);

            return Ok(result);
        }
    }
}
