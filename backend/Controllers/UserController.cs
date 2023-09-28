using backend.Models;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        [HttpGet("GetUser")]
        public async Task<ActionResult> GetUser(string id)
        {
            var user = await UserService.GetUserAsync(id);
            return Ok(user);
        }
        [HttpGet("GetCurrentUser")]
        public async Task<ActionResult> GetCurrentUser()
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var user = await UserService.GetUserAsync(userId);
            return Ok(user);
        }
        [HttpGet("GetUsers")]
        public async Task<ActionResult> GetUsers()
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            IEnumerable<UserBase>? users = await UserService.GetUsersAsync(userId);
            return Ok(users);
        }
        
        [HttpPost("UpdateUser")]
        public async Task<ActionResult> UpdateUser(User data)
        {
            var user = HttpContext.Items["user"] as User;
            if (user == null || string.IsNullOrEmpty(user.Id)) return Conflict("User or ID is null");


            // Check email
            if(string.IsNullOrEmpty(data.Email)) return BadRequest("Email null or empty");
            data.Email = data.Email.ToLower();

            if(user.Email != data.Email)
            {
                var addr = new System.Net.Mail.MailAddress(data.Email);
                if (addr.Address != data.Email) return BadRequest("Email not validate");
                var find = await UserService.FindUserByEmailAsync(data.Email);
                if (find != null) return Conflict("Email exist"); 
            }

            // Update user
            if(!string.IsNullOrEmpty(data.Born))
            {
                DateOnly born = DateOnly.Parse(data.Born.ToString());
                user.BirthDay = born.ToDateTime(TimeOnly.Parse("1:00 PM"));
                user.Born = born.ToLongDateString();
            }
            user.Email = data.Email;
            user.FirstName = data.FirstName;
            user.LastName = data.LastName;
            user.Phone = data.Phone;
            user.Gender = data.Gender;
            user.Status = Status.Active;
            user.FamilyStatus = data.FamilyStatus;
            user.AboutMe = data.AboutMe;
            user.Location = data.Location;
            user.Work = data.Work;
            await UserService.UpdateUserAsync(user.Id, user);
            return Ok("User is Updated");
        }

        [HttpPost("UpdateUserPassword")]
        public async Task<ActionResult> UpdateUserPassword(Request request)
        {
            if (string.IsNullOrEmpty(request.OldPassword) || string.IsNullOrEmpty(request.NewPassword)) return BadRequest("Data is null or empty");

            var user = HttpContext.Items["user"] as User;
            if (user == null || string.IsNullOrEmpty(user.Id)) return Conflict("User or ID is null");

            // Update user
            if (request.NewPassword.Length < 6) return BadRequest("Password less than 6 characters!");
            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password)) return Conflict("Passwords are not the same!");

            await UserService.UpdateUserPasswordAsync(user.Id, BCrypt.Net.BCrypt.HashPassword(request.NewPassword));

            string[] mailDescription = { "Восстановление пароля в", "Ваш пароль был успешно заменен", "Ваши данные при регистрации:" };
            await EmailService.SendEmailAsync(user.Email, "Смена пароля на Connections", user.FirstName, user.LastName, user.Joined, request.NewPassword, mailDescription);

            return Ok("User is Updated");
        }

        [HttpPost("SaveAvatar")]
        public async Task<ActionResult> SaveAvatar(IFormFile file)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var photo = await GalleryService.AddPhotoAsync(userId);

            var url = await UserService.SaveFileAsync(file, "Photos", photo.Key);
            if (url == null) return Conflict("Save avatar failed");

            Photo result = photo.Object;
            result.Id = photo.Key;
            result.Url = url;

            await GalleryService.UpdatePhotoAsync(userId, photo.Key, result);

            await UserService.UpdateUserAvatarUrlAsync(userId, url);
            return Ok("Ok");
        }
        [HttpPost("SaveBackground")]
        public async Task<ActionResult> SaveBackground(IFormFile file)
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) return Conflict("User id is null");

            var photo = await GalleryService.AddPhotoAsync(userId);

            var url = await UserService.SaveFileAsync(file, "Photos", photo.Key);
            if (url == null) return Conflict("Save background failed");

            Photo result = photo.Object;
            result.Id = photo.Key;
            result.Url = url;

            await GalleryService.UpdatePhotoAsync(userId, photo.Key, result);

            await UserService.UpdateUserBackgroundUrlAsync(userId, url);
            return Ok("Ok");
        }
    }
}
