using backend.Models;
using backend.Services;
using Firebase.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

                return Ok(GetResponse(find));
            }
            else return Conflict("User does not exist");
        }
        [HttpPost("SignUp")]
        public async Task<ActionResult> SignUp(string email, string password)
        {
            email = email.ToLower();
            if (password.Length < 6) return BadRequest("Password less than 6 characters!");
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email) return BadRequest("Email not validate");
            var find = await UserService.FindUserByEmailAsync(email);
            if (find != null && find.Object != null) return Conflict("User exists");
            else
            {
                User user = new User();
                user.Email = email;
                user.Role = Role.User;
                user.Status = Status.Active;
                user.LastVisit = DateTime.UtcNow;
                user.Password = BCrypt.Net.BCrypt.HashPassword(password);

                var result = await UserService.AddUserAsync(user);
                if (result.Object == null) return Conflict("Failed registration");

                user.Id = result.Key;
                await UserService.UpdateUserAsync(result.Key, user);

                return Ok("Registration successful");
            }
        }
        private static Response GetResponse(FirebaseObject<User> user)
        {
            List<Claim> claims = new();
            claims.Add(new Claim(ClaimTypes.PrimarySid, user.Key));
            claims.Add(new Claim(ClaimTypes.Email, user.Object.Email));
            claims.Add(new Claim(ClaimTypes.Role, user.Object.Role.ToString()));
            

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationManager.AppSetting["JWT:Secret"]));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var tokeOptions = new JwtSecurityToken(
                issuer: ConfigurationManager.AppSetting["JWT:ValidIssuer"],
                audience: ConfigurationManager.AppSetting["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: signinCredentials
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            return new Response(user.Object, tokenString);
        }
    }
}
