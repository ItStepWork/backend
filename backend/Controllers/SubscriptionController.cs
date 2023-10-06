using backend.Models;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SubscriptionController : Controller
    {
        [HttpGet("SubscribeToMessages")]
        public async Task SubscribeToMessages()
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId))
            {
                HttpContext.Response.StatusCode = 401;
                var buffer = Encoding.UTF8.GetBytes("User ID is null");
                await HttpContext.Response.Body.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                await SubscriptionService.SubscribeToMessagesAsync(this.HttpContext, "Messages", userId);
            }
        }
        [HttpGet("SubscribeToUserUpdates")]
        public async Task SubscribeToUserUpdates(string id)
        {
            await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Users/{id}", "Update user");
        }
        [HttpGet("SubscribeToMessagesUpdates")]
        public async Task SubscribeToMessagesUpdates()
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId))
            {
                HttpContext.Response.StatusCode = 401;
                var buffer = Encoding.UTF8.GetBytes("User ID is null");
                await HttpContext.Response.Body.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Messages/{userId}", "Update messages");
            }
        }
        [HttpGet("SubscribeToGroupsUpdates")]
        public async Task SubscribeToGroupsUpdates()
        {
            await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, "Groups", "Update groups");
        }
        [HttpGet("SubscribeToGroupUpdates")]
        public async Task SubscribeToGroupUpdates(string id)
        {
            Group? group = await GroupService.GetGroupAsync(id);
            if (group == null)
            {
                HttpContext.Response.StatusCode = 400;
                var buffer = Encoding.UTF8.GetBytes("Group not found");
                await HttpContext.Response.Body.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Groups/{id}", "Update group");
            }
        }
        [HttpGet("SubscribeToFriendRequest")]
        public async Task SubscribeToFriendRequest()
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId))
            {
                HttpContext.Response.StatusCode = 401;
                var buffer = Encoding.UTF8.GetBytes("User ID is null");
                await HttpContext.Response.Body.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                await SubscriptionService.SubscribeToNotificationAsync(this.HttpContext, userId, NotificationType.AddFriend, "Запрос в друзья");
            }
        }
        [HttpGet("SubscribeToFriendsUpdates")]
        public async Task SubscribeToFriendsUpdates()
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId))
            {
                HttpContext.Response.StatusCode = 401;
                var buffer = Encoding.UTF8.GetBytes("User ID is null");
                await HttpContext.Response.Body.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Friends/{userId}", "Update friends");
            }
        }
        [HttpGet("SubscribeToNotificationUpdates")]
        public async Task SubscribeToNotificationUpdates()
        {
            var userId = HttpContext.Items["userId"] as string;
            if (string.IsNullOrEmpty(userId))
            {
                HttpContext.Response.StatusCode = 401;
                var buffer = Encoding.UTF8.GetBytes("User ID is null");
                await HttpContext.Response.Body.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Notifications/{userId}", "Update notifications");
            }
        }
        [HttpGet("SubscribeToPostsUpdates")]
        public async Task SubscribeToPostsUpdates(string id)
        {
            var user = await UserService.GetUserAsync(id);
            var group = await GroupService.GetGroupAsync(id);
            if (user == null && group == null)
            {
                HttpContext.Response.StatusCode = 400;
                var buffer = Encoding.UTF8.GetBytes("ID not found");
                await HttpContext.Response.Body.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Posts/{id}", "Update posts");
            }
        }
        [HttpGet("SubscribeToStoriesUpdates")]
        public async Task SubscribeToStoriesUpdates(string id)
        {
            var user = await UserService.GetUserAsync(id);
            if (user == null)
            {
                HttpContext.Response.StatusCode = 400;
                var buffer = Encoding.UTF8.GetBytes("User not found");
                await HttpContext.Response.Body.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Stories/{id}", "Update stories");
            }
        }
        [HttpGet("SubscribeToAlbumsUpdates")]
        public async Task SubscribeToAlbumsUpdates(string id)
        {
            var user = await UserService.GetUserAsync(id);
            if (user == null)
            {
                HttpContext.Response.StatusCode = 400;
                var buffer = Encoding.UTF8.GetBytes("User not found");
                await HttpContext.Response.Body.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Albums/{id}", "Update albums");
            }
        }
        [HttpGet("SubscribeToPhotosUpdates")]
        public async Task SubscribeToPhotosUpdates(string id)
        {
            var user = await UserService.GetUserAsync(id);
            if (user == null)
            {
                HttpContext.Response.StatusCode = 400;
                var buffer = Encoding.UTF8.GetBytes("User not found");
                await HttpContext.Response.Body.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                await SubscriptionService.SubscribeUpdatesAsync(this.HttpContext, $"Photos/{id}", "Update photos");
            }
        }
    }
}
