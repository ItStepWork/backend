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
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) HttpContext.Response.StatusCode = 400;
            else
            {
                await SubscriptionService.SubscribeToMessagesAsync(this.HttpContext, "Messages", userId);
            }
        }
        [Authorize]
        [HttpGet("SubscribeToUserUpdates")]
        public async Task SubscribeToUserUpdates(string id)
        {
            await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Users/{id}", "Update user");
        }
        [Authorize]
        [HttpGet("SubscribeToMessagesUpdates")]
        public async Task SubscribeToMessagesUpdates()
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) HttpContext.Response.StatusCode = 400;
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Messages/{userId}", "Update messages");
            }
        }
        [Authorize]
        [HttpGet("SubscribeToGroupsUpdates")]
        public async Task SubscribeToGroupsUpdates()
        {
            await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, "Groups", "Update groups");
        }
        [Authorize]
        [HttpGet("SubscribeToGroupUpdates")]
        public async Task SubscribeToGroupUpdates(string id)
        {
            Group? group = await GroupService.GetGroupAsync(id);
            if (group == null) HttpContext.Response.StatusCode = 400;
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Groups/{id}", "Update group");
            }
        }
        [Authorize]
        [HttpGet("SubscribeToFriendRequest")]
        public async Task SubscribeToFriendRequest()
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) HttpContext.Response.StatusCode = 400;
            else
            {
                await SubscriptionService.SubscribeToNotificationAsync(this.HttpContext, userId, Models.Enums.NotificationType.AddFriend, "Запрос в друзья");
            }
        }
        [Authorize]
        [HttpGet("SubscribeToFriendsUpdates")]
        public async Task SubscribeToFriendsUpdates()
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) HttpContext.Response.StatusCode = 400;
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Friends/{userId}", "Update friends");
            }
        }
        [Authorize]
        [HttpGet("SubscribeToNotificationUpdates")]
        public async Task SubscribeToNotificationUpdates()
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId)) HttpContext.Response.StatusCode = 400;
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Notifications/{userId}", "Update notifications");
            }
        }
        [Authorize]
        [HttpGet("SubscribeToPostsUpdates")]
        public async Task SubscribeToPostsUpdates(string id)
        {
            var user = await UserService.GetUserAsync(id);
            var group = await GroupService.GetGroupAsync(id);
            if (user == null && group == null) HttpContext.Response.StatusCode = 400;
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Posts/{id}", "Update posts");
            }
        }
        [Authorize]
        [HttpGet("SubscribeToStoriesUpdates")]
        public async Task SubscribeToStoriesUpdates(string id)
        {
            var user = await UserService.GetUserAsync(id);
            if (user == null) HttpContext.Response.StatusCode = 400;
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Stories/{id}", "Update stories");
            }
        }
        [Authorize]
        [HttpGet("SubscribeToAlbumsUpdates")]
        public async Task SubscribeToAlbumsUpdates(string id)
        {
            var user = await UserService.GetUserAsync(id);
            if (user == null) HttpContext.Response.StatusCode = 400;
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Albums/{id}", "Update albums");
            }
        }
        [Authorize]
        [HttpGet("SubscribeToPhotosUpdates")]
        public async Task SubscribeToPhotosUpdates(string id)
        {
            var user = await UserService.GetUserAsync(id);
            if (user == null) HttpContext.Response.StatusCode = 400;
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Photos/{id}", "Update photos");
            }
        }
    }
}
