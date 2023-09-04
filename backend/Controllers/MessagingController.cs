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
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);
            if (resultValidate.user.Id == request.Id) return Conflict("Trying to send a message to yourself");

            User? recipient = await UserService.FindUserByIdAsync(request.Id);
            if (recipient == null) return NotFound("Recipient not found!");

            Message? message = await MessagingService.SendMessageAsync(resultValidate.user.Id, request);
            if (message == null || message.Id == null) return Conflict("Send message failed");

            return Ok("Ok");
        }
        [HttpGet("GetDialogs")]
        public async Task<ActionResult> GetDialogs()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await MessagingService.GetDialogs(resultValidate.user.Id);
            return Ok(result);
        }
        [HttpDelete("RemoveDialog")]
        public async Task<ActionResult> RemoveDialog(string id)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            await MessagingService.RemoveDialogAsync(resultValidate.user.Id, id);
            return Ok("Ok");
        }
        [HttpDelete("RemoveMessageFull")]
        public async Task<ActionResult> RemoveMessageFull(string id)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            await MessagingService.RemoveMessageAsync(resultValidate.user.Id, id, true);
            return Ok("Ok");
        }
        [HttpDelete("RemoveMessage")]
        public async Task<ActionResult> RemoveMessage(string id)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            await MessagingService.RemoveMessageAsync(resultValidate.user.Id, id, false);
            return Ok("Ok");
        }
        [HttpGet("GetMessages")]
        public async Task<ActionResult> GetMessages(string id)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            var result = await MessagingService.GetMessages(resultValidate.user.Id, id);
            return Ok(result);
        }
    }
}
