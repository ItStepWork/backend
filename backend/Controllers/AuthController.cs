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
        [HttpGet("Login")]
        public async Task<ActionResult> Login(string email, string password)
        {
            if (password.Length < 6) return BadRequest("Password less than 6 characters!");
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email) return BadRequest("Email not validate");
            var result = await UserService.FindUser(email);
            if (result?.Object == null) return Conflict("User does not exist");
            User user = result.Object;
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password)) return Conflict("Wrong data");

            return Ok(GetJWTToken(result));
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

            return Ok(GetJWTToken(result));
        }

        private static JWTTokenResponse GetJWTToken(FirebaseObject<User> user)
        {
            List<Claim> claims = new();
            claims.Add(new Claim(ClaimTypes.PrimarySid, user.Key));
            claims.Add(new Claim(ClaimTypes.Email, user.Object.Email));

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
            return new JWTTokenResponse { Token = tokenString };
        }
    }
}
