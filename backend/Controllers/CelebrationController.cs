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
            await Console.Out.WriteLineAsync(request.Date.ToString());
            if (request == null ) return BadRequest("Audience or File is null");
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");
            await CelebrationService.AddEventAsync(request, userId);
            return Ok("Added");
        }
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
