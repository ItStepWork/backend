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
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var user = await UserService.GetUserAsync(id);
            return Ok(user);
        }
        [HttpGet("GetCurrentUser")]
        public async Task<ActionResult> GetCurrentUser()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var user = await UserService.GetUserAsync(resultValidate.user.Id);
            return Ok(user);
        }
        [HttpGet("GetUsers")]
        public async Task<ActionResult> GetUsers()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            IEnumerable<UserBase>? users = await UserService.GetUsersAsync(resultValidate.user.Id);
            return Ok(users);
        }
        
        [HttpPost("UpdateUser")]
        public async Task<ActionResult> UpdateUser(User data)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            User user = resultValidate.user;

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
            await UserService.UpdateUserAsync(resultValidate.user.Id, user);
            return Ok("User is Updated");
        }

        [HttpPost("UpdateUserPassword")]
        public async Task<ActionResult> UpdateUserPassword(Request data)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            User user = resultValidate.user;

            // Update user
            if (data.OldPassword != null)
            {
                if (data.NewPassword.Length < 6) return BadRequest("Password less than 6 characters!");
                if (!BCrypt.Net.BCrypt.Verify(data.OldPassword, user.Password)) return Conflict("Passwords are not the same!");
               
                user.Password = BCrypt.Net.BCrypt.HashPassword(data.NewPassword);
                string[] mailDescription = { "Восстановление пароля в", "Ваш пароль был успешно заменен", "Ваши данные при регистрации:" };
                await EmailService.SendEmailAsync(user.Email, "Смена пароля на Connections", user.FirstName, user.LastName, user.Joined, data.NewPassword, mailDescription);
            }

            await UserService.UpdateUserAsync(resultValidate.user.Id, user);

            return Ok("User is Updated");
        }

        [HttpPost("SaveAvatar")]
        public async Task<ActionResult> SaveAvatar(IFormFile file)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var photo = await GalleryService.AddPhotoAsync(resultValidate.user.Id);

            var url = await UserService.SaveFileAsync(file, "Photos", photo.Key);
            if (url == null) return Conflict("Save avatar failed");

            Photo result = photo.Object;
            result.Id = photo.Key;
            result.Url = url;

            await GalleryService.UpdatePhotoAsync(resultValidate.user.Id, photo.Key, result);

            resultValidate.user.AvatarUrl = url;

            await UserService.UpdateUserAsync(resultValidate.user.Id, resultValidate.user);

            return Ok(resultValidate.user);
        }
        [HttpPost("SaveBackground")]
        public async Task<ActionResult> SaveBackground(IFormFile file)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var photo = await GalleryService.AddPhotoAsync(resultValidate.user.Id);

            var url = await UserService.SaveFileAsync(file, "Photos", photo.Key);
            if (url == null) return Conflict("Save background failed");

            Photo result = photo.Object;
            result.Id = photo.Key;
            result.Url = url;

            await GalleryService.UpdatePhotoAsync(resultValidate.user.Id, photo.Key, result);

            resultValidate.user.BackgroundUrl = url;

            await UserService.UpdateUserAsync(resultValidate.user.Id, resultValidate.user);

            return Ok(resultValidate.user);
        }
    }
}
