using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        [Authorize]
        [HttpPost("SendMessage")]
        public async Task<ActionResult> SendMessage(string userId, string text)
        {
            Claim? claimId = this.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid);
            if (claimId == null) return NotFound("User not authorize!");

            User? user = await UserService.FindUserById(claimId.Value);
            if (user == null) return NotFound("Sender not found!");

            User? recipient = await UserService.FindUserById(userId);
            if (user == null) return NotFound("Recipient not found!");

            Message? message = await UserService.SendMessage(claimId.Value, userId, text);
            if (message == null) return Conflict("Send message failed");

            return Ok("Ok");
        }
    }
}
