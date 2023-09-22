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
            return Ok(result);
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
    }
}
