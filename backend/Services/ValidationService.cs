using backend.Models;
using backend.Models.Enums;
using System.Security.Claims;

namespace backend.Services
{
    public static class ValidationService
    {
        public static async Task<(int status, string response, User? user)> ValidationUser(HttpContext httpContext)
        {
            Claim? claimId = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid);
            if (claimId == null) return (401, "User not authorize!", null);

            User? sender = await UserService.FindUserByIdAsync(claimId.Value);
            if (sender == null) return (401, "Sender not found!", null);
            if (sender.BlockingTime > DateTime.UtcNow) return (409, General.GetBlockingTime(sender.BlockingTime), null);

            var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
            var ipAddress = remoteIpAddress?.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
                    ? remoteIpAddress.MapToIPv4().ToString()
                    : remoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ipAddress) && ipAddress != "127.0.0.1" && ipAddress != "0.0.0.1") await UserService.UpdateUserIpAddressAsync(claimId.Value, ipAddress);
            await UserService.UpdateUserLastVisitAsync(claimId.Value);

            var path = httpContext.Request.Path;
            if (path.HasValue)
            {
                Page? page = null;
                if (path.Value.StartsWith("/Friend/")) page = Page.Contacts;
                else if (path.Value.StartsWith("/Messaging/")) page = Page.Messaging;
                else if (path.Value.StartsWith("/Gallery/")) page = Page.Gallery;
                else if (path.Value.StartsWith("/Notification/")) page = Page.Notifications;
                else if (path.Value.StartsWith("/Group/")) page = Page.Groups;
                else if (path.Value.StartsWith("/Auth/")) page = Page.Authorization;
                if (page != null)
                {
                    Activity activity = new();
                    activity.Page = page;
                    activity.UserId = sender.Id;
                    activity.DateTime = DateTime.UtcNow;
                    await ActivityService.AddActivityAsync(activity);
                }
            }

            return (200, "Ok", sender);
        }
        public static async Task<(int status, string response, User? user)> ValidationAdmin(HttpContext httpContext, Role role)
        {
            Claim? claimRole = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            Claim? claimId = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid);
            if (claimId == null || claimRole == null) return (401, "User not authorize!", null);

            User? sender = await UserService.FindUserByIdAsync(claimId.Value);
            if (sender == null) return (401, "Sender not found!", null);
            if (sender.Role.ToString() != claimRole.Value) return (401, "User role changed", null);

            if (role == Role.Moderator && sender.Role == Role.User) return (409, "User not access!", null);
            else if (role == Role.Admin && sender.Role != Role.Admin) return (409, "User not access!", null);

            if (sender.BlockingTime > DateTime.UtcNow) return (409, General.GetBlockingTime(sender.BlockingTime), null);

            var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
            var ipAddress = remoteIpAddress?.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
                    ? remoteIpAddress.MapToIPv4().ToString()
                    : remoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ipAddress) && ipAddress != "127.0.0.1" && ipAddress != "0.0.0.1") await UserService.UpdateUserIpAddressAsync(claimId.Value, ipAddress);
            await UserService.UpdateUserLastVisitAsync(claimId.Value);
            return (200, "Ok", sender);
        }
    }
}
