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
        public async Task<ActionResult> GetPagesActivity()
        {
            var resultValidate = await AdminService.ValidationAdmin(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await AdminService.GetPagesActivityAsync();
            return Ok(result);
        }
        [HttpGet("GetUsersActivity")]
        public async Task<ActionResult> GetUsersActivity()
        {
            var resultValidate = await AdminService.ValidationAdmin(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await AdminService.GetUsersActivityAsync();
            return Ok(result);
        }
    }
}
