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
            var result = await CelebrationService.GetBirthdaysEventNowAsync(id);
            return Ok(result);
        }
        [HttpGet("GetBirthdaysSoon")]
        public async Task<ActionResult> GetBirthdaysSoon(string id)
        {
            var result = await CelebrationService.GetBirthdaysEventSoonAsync(id);
            return Ok(result);
        }
    }
}
