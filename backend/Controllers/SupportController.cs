using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SupportController : Controller
    {
        [HttpPost("SendMessage")]
        public async Task<ActionResult> SendMessage([FromForm] Request request)
        {
            if (string.IsNullOrEmpty(request.Text)) return BadRequest("Text is null or empty");

            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            await SupportService.SendMessageAsync(userId, request);
            return Ok("Ok");
        }
        [HttpGet("GetMessages")]
        public async Task<ActionResult> GetMessages()
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var result = await SupportService.GetMessagesAsync(userId);
            return Ok(result);
        }
        [HttpPost("SendComplaint")]
        public async Task<ActionResult> SendComplaint([FromForm] Request request)
        {
            if (string.IsNullOrEmpty(request.Text)) return BadRequest("Text is null or empty");

            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            await SupportService.SendComplaintAsync(userId, request);
            return Ok("Ok");
        }
    }
}
