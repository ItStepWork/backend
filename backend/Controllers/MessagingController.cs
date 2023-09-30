using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessagingController : Controller
    {
        [HttpPost("SendMessage")]
        public async Task<ActionResult> SendMessage([FromForm] Request request)
        {
            if (string.IsNullOrEmpty(request.Text) || string.IsNullOrEmpty(request.Id)) return BadRequest("Data is null or empty");
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");
            if (userId == request.Id) return Conflict("Trying to send a message to yourself");

            User? recipient = await UserService.FindUserByIdAsync(request.Id);
            if (recipient == null) return NotFound("Recipient not found!");

            Message? message = await MessagingService.SendMessageAsync(userId, request);
            if (message == null || message.Id == null) return Conflict("Send message failed");

            return Ok("Ok");
        }
        [HttpGet("GetDialogs")]
        public async Task<ActionResult> GetDialogs()
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var result = await MessagingService.GetDialogs(userId);
            return Ok(result);
        }
        [HttpDelete("RemoveDialog")]
        public async Task<ActionResult> RemoveDialog(string id)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            await MessagingService.RemoveDialogAsync(userId, id);
            return Ok("Ok");
        }
        [HttpDelete("RemoveMessageFull")]
        public async Task<ActionResult> RemoveMessageFull(string id)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            await MessagingService.RemoveMessageAsync(userId, id, true);
            return Ok("Ok");
        }
        [HttpDelete("RemoveMessage")]
        public async Task<ActionResult> RemoveMessage(string id)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            await MessagingService.RemoveMessageAsync(userId, id, false);
            return Ok("Ok");
        }
        [HttpGet("GetMessages")]
        public async Task<ActionResult> GetMessages(string id)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            var result = await MessagingService.GetMessages(userId, id);
            return Ok(result);
        }
        [HttpPost("UpdateMessageStatus")]
        public async Task<ActionResult> UpdateMessageStatus(Request request)
        {
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Id)) return BadRequest("Data is null or empty");
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");
            if (userId == request.UserId) BadRequest("Update message status failed");

            await MessagingService.UpdateMessageStatusAsync(userId, request.UserId, request.Id);
            return Ok("Ok");
        }
    }
}
