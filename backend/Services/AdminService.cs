using backend.Models;
using System.Security.Claims;

namespace backend.Services
{
    public static class AdminService
    {
        public static async Task<(string response, User? user)> ValidationAdmin(HttpContext httpContext)
        {
            Claim? claimRole = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            Claim? claimId = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid);
            if (claimId == null || claimRole == null) return ("User not authorize!", null);

            User? sender = await UserService.FindUserByIdAsync(claimId.Value);
            if (sender == null) return ("Sender not found!", null);

            if (sender.Role != Models.Enums.Role.Admin || sender.Role.ToString() != claimRole.Value) return ("User not access!", null);

            sender.LastVisit = DateTime.UtcNow;
            await UserService.UpdateUserAsync(claimId.Value, sender);
            return ("", sender);
        }
    }
}
