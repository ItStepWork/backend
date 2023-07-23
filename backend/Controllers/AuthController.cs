using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        [HttpPost("Login")]
        public async Task<ActionResult> Login(string email, string password)
        {
            if (password.Length < 6) return BadRequest("Password less than 6 characters!");
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email) return BadRequest("Email not validate");
            var result = await UserService.FindUser(email);
            if (result?.Object == null) return Conflict("User does not exist");
            User user = result.Object;
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password)) return Conflict("Wrong data");
            return Ok("Ok");
        }
        [HttpPost("Registration")]
        public async Task<ActionResult> Registration(string email, string password)
        {
            if (password.Length < 6) return BadRequest("Password less than 6 characters!");
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email) return BadRequest("Email not validate");
            var find = await UserService.FindUser(email);
            if (find != null && find.Object != null) return Conflict("User exists");
            User user = new User();
            user.Email = email;
            user.Password = BCrypt.Net.BCrypt.HashPassword(password);
            var result = await UserService.Add(user);
            if (result.Object == null) return Conflict("Failed registration");
            else return Ok("Ok");
        }
    }
}
