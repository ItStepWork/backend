using backend.Models;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : Controller
    {
        [HttpGet("GetUsers")]
        public async Task<ActionResult> GetUsers()
        {
            var resultValidate = await AdminService.ValidationAdmin(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await UserService.GetUsersAsync();
            var sortModerator = result?.OrderByDescending(user => user.Role == Role.Moderator);
            var sortAdmin = sortModerator?.OrderByDescending(user=>user.Role == Role.Admin);
            return Ok(sortAdmin);
        }
        [HttpGet("GetAllActivity")]
        public async Task<ActionResult> GetAllActivity()
        {
            var resultValidate = await AdminService.ValidationAdmin(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await ActivityService.GetAllActivityAsync();
            return Ok(result);
        }
        [HttpGet("GetPagesActivity")]
        public async Task<ActionResult> GetPagesActivity(Chart chart)
        {
            var resultValidate = await AdminService.ValidationAdmin(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await AdminService.GetPagesActivityAsync(chart);
            return Ok(result);
        }
        [HttpGet("GetUsersActivity")]
        public async Task<ActionResult> GetUsersActivity(Chart chart)
        {
            var resultValidate = await AdminService.ValidationAdmin(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await AdminService.GetUsersActivityAsync(chart);
            return Ok(result);
        }
        [HttpPost("UpdateUserStatus")]
        public async Task<ActionResult> UpdateUserStatus(Request request)
        {
            if (request.Status == null || string.IsNullOrEmpty(request.UserId)) return BadRequest("Data is null or empty");
            var resultValidate = await AdminService.ValidationAdmin(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            await UserService.UpdateUserStatusAsync(request.UserId, (Status)request.Status);
            return Ok("Ok");
        }
        [HttpPost("UpdateUserRole")]
        public async Task<ActionResult> UpdateUserRole(Request request)
        {
            if (request.Role == null || string.IsNullOrEmpty(request.UserId)) return BadRequest("Data is null or empty");
            var resultValidate = await AdminService.ValidationAdmin(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            await UserService.UpdateUserRoleAsync(request.UserId, (Role)request.Role);
            return Ok("Ok");
        }
        [HttpPost("UpdateUserBlockingTime")]
        public async Task<ActionResult> UpdateUserBlockingTime(Request request)
        {
            if (string.IsNullOrEmpty(request.BlockingTime) || string.IsNullOrEmpty(request.UserId)) return BadRequest("Data is null or empty");
            var resultValidate = await AdminService.ValidationAdmin(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            await UserService.UpdateUserBlockingTimeAsync(request.UserId, DateTime.Parse(request.BlockingTime).ToUniversalTime());
            return Ok("Ok");
        }
    }
}
