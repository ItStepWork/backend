using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SubscriptionController : Controller
    {
        [Authorize]
        [HttpGet("SubscribeToMessages")]
        public async Task SubscribeToMessages()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) HttpContext.Response.StatusCode = 400;
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, "Messages", resultValidate.user.Id);
            }
        }
        [Authorize]
        [HttpGet("SubscribeToMessagesUpdates")]
        public async Task SubscribeToMessagesUpdates()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) HttpContext.Response.StatusCode = 400;
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Messages/{resultValidate.user.Id}", "Update messages");
            }
        }
        [Authorize]
        [HttpGet("SubscribeToGroupsUpdates")]
        public async Task SubscribeToGroupsUpdates()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) HttpContext.Response.StatusCode = 400;
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, "Groups", "Update groups");
            }
        }
        [Authorize]
        [HttpGet("SubscribeToGroupUpdates")]
        public async Task SubscribeToGroupUpdates(string id)
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) HttpContext.Response.StatusCode = 400;
            else
            {
                Group? group = await GroupService.GetGroupAsync(id);
                if(group == null) HttpContext.Response.StatusCode = 400;
                else
                {
                    await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Groups/{id}", "Update group");
                }
            }
        }
        [Authorize]
        [HttpGet("SubscribeToFriendRequest")]
        public async Task SubscribeToFriendRequest()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) HttpContext.Response.StatusCode = 400;
            else
            {
                await SubscriptionService.SubscribeToFriendRequestAsync(this.HttpContext, "Friends", resultValidate.user.Id);
            }
        }
        [Authorize]
        [HttpGet("SubscribeToFriendsUpdates")]
        public async Task SubscribeToFriendsUpdates()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) HttpContext.Response.StatusCode = 400;
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Friends/{resultValidate.user.Id}", "Update friends");
            }
        }
        [Authorize]
        [HttpGet("SubscribeToNotificationUpdates")]
        public async Task SubscribeToNotificationUpdates()
        {
            var resultValidate = await UserService.ValidationUser(this.HttpContext);
            if (resultValidate.user == null || resultValidate.user.Id == null) HttpContext.Response.StatusCode = 400;
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Notifications/{resultValidate.user.Id}", "Update notifications");
            }
        }
    }
}
