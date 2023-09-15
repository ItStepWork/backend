using backend.Models;
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
    }
}
