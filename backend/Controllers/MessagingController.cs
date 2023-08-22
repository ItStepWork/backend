using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessagingController : Controller
    {
        [HttpPost("SendMessage")]
        public async Task<ActionResult> SendMessage([FromForm] MessageRequest data)
        {
            if (string.IsNullOrEmpty(data.Text)) return BadRequest("Text is null or empty");
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            User? recipient = await UserService.FindUserByIdAsync(data.Id);
            if (recipient == null) return NotFound("Recipient not found!");

            Message? message = await MessagingService.SendMessageAsync(resultValidate.user.Id, data);
            if (message == null || message.Id == null) return Conflict("Send message failed");

            return Ok("Ok");
        }
        [HttpGet("GetDialogs")]
        public async Task<ActionResult> GetDialogs()
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await MessagingService.GetDialogs(resultValidate.user.Id);
            return Ok(result);
        }
        [HttpDelete("RemoveDialog")]
        public async Task<ActionResult> RemoveDialog(string id)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            await MessagingService.RemoveDialogAsync(resultValidate.user.Id, id);
            return Ok("Ok");
        }
        [HttpGet("GetMessages")]
        public async Task<ActionResult> GetMessages(string id)
        {
            (string response, User? user) resultValidate = await ValidationUser();
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            User? recipient = await UserService.FindUserByIdAsync(id);
            if (recipient == null) return NotFound("Recipient not found!");

            var result = await MessagingService.GetMessages(resultValidate.user.Id, id);
            return Ok(result);
        }
        private async Task<(string, User?)> ValidationUser()
        {
            Claim? claimId = this.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid);
            if (claimId == null) return ("User not authorize!", null);

            User? sender = await UserService.FindUserByIdAsync(claimId.Value);
            if (sender == null) return ("Sender not found!", null);

            sender.LastVisit = DateTime.UtcNow;
            await UserService.UpdateUserAsync(claimId.Value, sender);
            return ("", sender);
        }
    }
}
