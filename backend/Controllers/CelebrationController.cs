using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CelebrationController : Controller
    {
        [HttpPost("AddEvent")]
        public async Task<ActionResult> AddEvent(Request request)
        {
            if (request == null ) return BadRequest("Audience or File is null");
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");
            await CelebrationService.AddEventAsync(request, userId);
            return Ok("Added");
        }
        [HttpGet("GetBirthdaysNow")]
        public async Task<ActionResult> GetBirthdaysNow()
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");
            var result = await CelebrationService.GetBirthdaysEventNowAsync(userId);
            return Ok(result);
        }
        [HttpGet("GetBirthdaysSoon")]
        public async Task<ActionResult> GetBirthdaysSoon()
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");
            var result = await CelebrationService.GetBirthdaysEventSoonAsync(userId);
            return Ok(result);
        }
    }
}
