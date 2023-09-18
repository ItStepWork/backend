using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;

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
        [HttpGet("GetDailyPagesActivityChart")]
        public async Task<ActionResult> GetDailyPagesActivityChart()
        {
            var resultValidate = await AdminService.ValidationAdmin(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await AdminService.GetDailyPagesActivityChartAsync();
            return Ok(result);
        }
        [HttpGet("GetDailyActivityChart")]
        public async Task<ActionResult> GetDailyActivityChart()
        {
            var resultValidate = await AdminService.ValidationAdmin(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) return Unauthorized(resultValidate.response);

            var result = await AdminService.GetDailyActivityChartAsync();
            return Ok(result);
        }
    }
}
