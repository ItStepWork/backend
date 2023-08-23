using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        [HttpGet("SignIn")]
        public async Task<ActionResult> SignIn(string email, string password)
        {
            if (password.Length < 6) return BadRequest("Password less than 6 characters!");
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email) return BadRequest("Email not validate");
            var find = await UserService.FindUserByEmailAsync(email);
            if (find != null && find.Object != null)
            {
                User user = find.Object;
                if (!BCrypt.Net.BCrypt.Verify(password, user.Password)) return Conflict("Wrong data");

                user.LastVisit = DateTime.UtcNow;
                await UserService.UpdateUserAsync(find.Key, user);
                var response = UserService.GetToken(find);
                return Ok(response);
            }
            else return Conflict("User does not exist");
        }
        [HttpPost("SignUp")]
        public async Task<ActionResult> SignUp(User userData)
        { 
            if (userData == null ||
                string.IsNullOrEmpty(userData.Email) || 
                string.IsNullOrEmpty(userData.Password) || 
                string.IsNullOrEmpty(userData.FirstName) || 
                string.IsNullOrEmpty(userData.LastName)) return BadRequest("User data null or empty");
            
            userData.Email = userData.Email.ToLower();
            if (userData.Password.Length < 6) return BadRequest("Password less than 6 characters!");
            var addr = new System.Net.Mail.MailAddress(userData.Email);
            if (addr.Address != userData.Email) return BadRequest("Email not validate");
            var find = await UserService.FindUserByEmailAsync(userData.Email);
            if (find != null && find.Object != null) return Conflict("User exists");
            else
            {
                User user = new User();
                user.Email = userData.Email;
                user.FirstName = userData.FirstName;
                user.LastName = userData.LastName;
                user.Role = Role.User;
                user.Status = Status.Active;
                user.LastVisit = DateTime.UtcNow;
                user.Joined = DateOnly.FromDateTime(DateTime.Now).ToLongDateString();
                user.Password = BCrypt.Net.BCrypt.HashPassword(userData.Password);

                var result = await UserService.AddUserAsync(user);
                if (result.Object == null) return Conflict("Failed registration");

                user.Id = result.Key;
                await UserService.UpdateUserAsync(result.Key, user);

                return Ok("Registration successful");
            }
        }
    }
}
