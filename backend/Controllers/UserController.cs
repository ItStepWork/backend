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
        [HttpPost("GetUsers")]
        public async Task<ActionResult> GetUsers()
        {
            IEnumerable<UserBase>? users = await UserService.GetUsers();
            return Ok(users);
        }
        [Authorize]
        [HttpPost("SendMessage")]
        public async Task<ActionResult> SendMessage(string userId, string text)
        {
            Claim? claimId = this.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid);
            if (claimId == null) return NotFound("User not authorize!");

            User? sender = await UserService.FindUserById(claimId.Value);
            if (sender == null) return NotFound("Sender not found!");

            User? recipient = await UserService.FindUserById(userId);
            if (recipient == null) return NotFound("Recipient not found!");

            Message? message = await UserService.SendMessage(claimId.Value, userId, text);
            if (message == null) return Conflict("Send message failed");

            return Ok("Ok");
        }
    }
}
