using backend.Models;
using backend.Models.Enums;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using Microsoft.IdentityModel.Tokens;
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

            if(user != null)
            {
                var lastVisit = await GetUserLastVisitAsync(userId);
                if (lastVisit != null) user.LastVisit = DateTime.Parse(lastVisit);
            }

            return user;
        }
        public static async Task<IEnumerable<UserBase>?> GetUsersAsync()
        {
            var users = await firebaseDatabase
              .Child("Users")
              .OnceAsync<UserBase>();

            var lastVisits = await GetUsersLastVisitsAsync();

            return users?.Select(x => x.Object).Select(u => { if (u.Id != null && lastVisits.ContainsKey(u.Id)) u.LastVisit = DateTime.Parse(lastVisits[u.Id]); return u; });
        }
        public static async Task<IEnumerable<UserBase>?> GetUsersAsync(string userId)
        {
            var users = await firebaseDatabase
              .Child("Users")
              .OnceAsync<UserBase>();

            var lastVisits = await GetUsersLastVisitsAsync();

            return users?.Where(u=>u.Key != userId).Select(x => x.Object).Select(u => { if (u.Id != null && lastVisits.ContainsKey(u.Id)) u.LastVisit = DateTime.Parse(lastVisits[u.Id]); return u; });
        }
        public static async Task<List<UserBase>?> GetUsersAsync(string[] users)
        {
            var result = await firebaseDatabase
              .Child("Users")
              .OnceAsync<UserBase>();

            var lastVisits = await GetUsersLastVisitsAsync();

            return result?.Where(u => users.Contains(u.Key)).Select(x => x.Object).Select(u => { if (u.Id != null && lastVisits.ContainsKey(u.Id)) u.LastVisit = DateTime.Parse(lastVisits[u.Id]); return u; }).ToList();
        }
        public static async Task UpdateUserAsync(string userId, User user)
        {
            await firebaseDatabase
              .Child("Users")
              .Child(userId)
              .PutAsync(user);
        }
        public static async Task UpdateUserBlockingTimeAsync(string userId, DateTime dateTime)
        {
            await firebaseDatabase
              .Child("Users")
              .Child(userId)
              .Child("BlockingTime")
              .PutAsync<string>(dateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK"));
        }
        public static async Task UpdateUserRoleAsync(string userId, Role role)
        {
            await firebaseDatabase
              .Child("Users")
              .Child(userId)
              .Child("Role")
              .PutAsync<int>((int)role);
        }
        public static async Task UpdateUserStatusAsync(string userId, Status status)
        {
            await firebaseDatabase
              .Child("Users")
              .Child(userId)
              .Child("Status")
              .PutAsync<int>((int)status);
        }
        public static async Task UpdateUserPasswordAsync(string userId, string password)
        {
            await firebaseDatabase
              .Child("Users")
              .Child(userId)
              .Child("Password")
              .PutAsync<string>(password);
        }
        public static async Task UpdateUserBackgroundUrlAsync(string userId, string backgroundUrl)
        {
            await firebaseDatabase
              .Child("Users")
              .Child(userId)
              .Child("BackgroundUrl")
              .PutAsync<string>(backgroundUrl);
        }
        public static async Task UpdateUserAvatarUrlAsync(string userId, string avatarUrl)
        {
            await firebaseDatabase
              .Child("Users")
              .Child(userId)
              .Child("AvatarUrl")
              .PutAsync<string>(avatarUrl);
        }
        public static async Task UpdateUserIpAddressAsync(string userId, string ipAddress)
        {
            await firebaseDatabase
              .Child("Users")
              .Child(userId)
              .Child("IpAddress")
              .PutAsync<string>(ipAddress);
        }
        public static async Task UpdateUserLastVisitAsync(string userId)
        {
            await firebaseDatabase
              .Child("LastVisits")
              .Child(userId)
              .PutAsync<string>(DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK"));
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

            var user = users?.FirstOrDefault(user => user.Object.Email == email);
            if (user != null && user.Object.Id != null)
            {
                var lastVisit = await GetUserLastVisitAsync(user.Object.Id);
                if (lastVisit != null) user.Object.LastVisit = DateTime.Parse(lastVisit);
            }
            return user;
        }
        public static async Task<User?> FindUserByIdAsync(string userId)
        {
            var user = await firebaseDatabase
              .Child("Users")
              .Child(userId).OnceSingleAsync<User>();

            if (user != null && user.Id != null)
            {
                var lastVisit = await GetUserLastVisitAsync(user.Id);
                if (lastVisit != null) user.LastVisit = DateTime.Parse(lastVisit);
            }
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
        public static async Task<Dictionary<string, string>> GetUsersLastVisitsAsync()
        {
            var lastVisits = await firebaseDatabase.Child("LastVisits").OnceSingleAsync<Dictionary<string, string>>();
            return lastVisits;
        }
        public static async Task<string?> GetUserLastVisitAsync(string id)
        {
            var lastVisit = await firebaseDatabase
                  .Child("LastVisits")
                  .Child(id).OnceSingleAsync<string>();
            return lastVisit;
        }
    }
}
