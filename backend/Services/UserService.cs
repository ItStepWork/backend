using backend.Models;
using backend.Models.Enums;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Services
{
    public static class UserService
    {
        private static readonly FirebaseClient firebaseDatabase = new FirebaseClient("https://database-50f39-default-rtdb.europe-west1.firebasedatabase.app/");
        private static readonly FirebaseStorage firebaseStorage = new FirebaseStorage("database-50f39.appspot.com");

        public static Response GetToken(FirebaseObject<User> user)
        {
            List<Claim> claims = new();
            claims.Add(new Claim(ClaimTypes.PrimarySid, user.Key));
            claims.Add(new Claim(ClaimTypes.Email, user.Object.Email));
            claims.Add(new Claim(ClaimTypes.Role, user.Object.Role.ToString()));

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationManager.AppSetting["JWT:Secret"]));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var tokeOptions = new JwtSecurityToken(
                issuer: ConfigurationManager.AppSetting["JWT:ValidIssuer"],
                audience: ConfigurationManager.AppSetting["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: signinCredentials
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            return new Response(user.Object, tokenString);
        }
        public static async Task<(string response, User? user)> ValidationUser(HttpContext httpContext)
        {

            Claim? claimId = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid);
            if (claimId == null) return ("User not authorize!", null);

            User? sender = await FindUserByIdAsync(claimId.Value);
            if (sender == null) return ("Sender not found!", null);

            var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
            var ipAddress = remoteIpAddress?.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
                    ? remoteIpAddress.MapToIPv4().ToString()
                    : remoteIpAddress?.ToString();
            if(ipAddress != "0.0.0.1") sender.IpAddress = ipAddress;
            sender.LastVisit = DateTime.UtcNow;
            await UpdateUserAsync(claimId.Value, sender);

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

            return ("", sender);
        }
        public static async Task<FirebaseObject<User>> AddUserAsync(User user)
        {
            return await firebaseDatabase
              .Child("Users")
              .PostAsync(user);
        }
        public static async Task<UserBase?> GetUserAsync(string userId)
        {
            var user = await firebaseDatabase
              .Child("Users")
              .Child(userId).OnceSingleAsync<UserBase>();

            return user;
        }
        public static async Task<IEnumerable<UserBase>?> GetUsersAsync()
        {
            var users = await firebaseDatabase
              .Child("Users")
              .OnceAsync<UserBase>();

            return users?
              .Select(x => x.Object);
        }
        public static async Task<IEnumerable<UserBase>?> GetUsersAsync(string userId)
        {
            var users = await firebaseDatabase
              .Child("Users")
              .OnceAsync<UserBase>();

            return users?
              .Where(u=>u.Key != userId).Select(x => x.Object);
        }
        public static async Task<List<UserBase>?> GetUsersAsync(string[] users)
        {
            var result = await firebaseDatabase
              .Child("Users")
              .OnceAsync<UserBase>();

            return result?
              .Where(u => users.Contains(u.Key)).Select(x => x.Object).ToList();
        }
        public static async Task UpdateUserAsync(string userId, User user)
        {
            await firebaseDatabase
              .Child("Users")
              .Child(userId)
              .PutAsync(user);
        }
        public static async Task RemoveUserAsync(string userId)
        {
            await firebaseDatabase
              .Child("Users")
              .Child(userId)
              .DeleteAsync();
        }
        public static async Task<FirebaseObject<User>?> FindUserByEmailAsync(string email)
        {
            var users = await firebaseDatabase
              .Child("Users")
              .OnceAsync<User>();

            return users?
              .FirstOrDefault(user => user.Object.Email == email);
        }
        public static async Task<User?> FindUserByIdAsync(string userId)
        {
            var user = await firebaseDatabase
              .Child("Users")
              .Child(userId).OnceSingleAsync<User>();

            return user;
        }

        public static async Task<string?> SaveFileAsync(IFormFile file, string child, string name)
        {
            var stream = file.OpenReadStream();
            var task = await firebaseStorage
                 .Child(child)
                 .Child(name + ".png")
                 .PutAsync(stream);
            return task;
        }
        public static async Task RemoveFileAsync(string child, string name)
        {
            await firebaseStorage
                 .Child(child)
                 .Child(name + ".png")
                 .DeleteAsync();
        }
    }
}
