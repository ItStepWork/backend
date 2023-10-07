using backend.Models.Enums;
using backend.Services;
using System.Security.Claims;
using System.Text;

namespace backend.Middleware
{
    public sealed class ValidationHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        public ValidationHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;
            if (path.HasValue)
            {
                if (path.Value.StartsWith("/Admin/"))
                {
                    Role? role = null;
                    if (path.Value == "/Admin/GetUsers") role = Role.Moderator;
                    else if (path.Value == "/Admin/GetGroups") role = Role.Moderator;
                    else if (path.Value == "/Admin/GetAllActivity") role = Role.Moderator;
                    else if (path.Value == "/Admin/GetPagesActivity") role = Role.Moderator;
                    else if (path.Value == "/Admin/GetUsersActivity") role = Role.Moderator;
                    else if (path.Value == "/Admin/GetSupportDialogs") role = Role.Moderator;
                    else if (path.Value == "/Admin/GetSupportMessages") role = Role.Moderator;
                    else if (path.Value == "/Admin/SendSupportMessage") role = Role.Moderator;
                    else if (path.Value == "/Admin/GetComplaints") role = Role.Moderator;
                    else if (path.Value == "/Admin/UpdateComplaintStatus") role = Role.Admin;
                    else if (path.Value == "/Admin/UpdateUserStatus") role = Role.Admin;
                    else if (path.Value == "/Admin/UpdateUserRole") role = Role.Admin;
                    else if (path.Value == "/Admin/UpdateUserBlockingTime") role = Role.Admin;
                    else if (path.Value == "/Admin/UpdateGroupStatus") role = Role.Admin;
                    else if (path.Value == "/Admin/UpdateGroupBlockingTime") role = Role.Admin;
                    else if (path.Value == "/Admin/UpdatePostStatus") role = Role.Admin;

                    if (role != null)
                    {
                        var (status, response, user) = await ValidationService.ValidationAdmin(context, (Role)role);
                        if (user == null || string.IsNullOrEmpty(user.Id))
                        {
                            context.Response.StatusCode = status;
                            var buffer = Encoding.UTF8.GetBytes(response);
                            await context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
                        }
                        else
                        {
                            context.Items["user"] = user;
                            context.Items["userId"] = user.Id;
                            await _next(context);
                        }
                    }
                    else await _next(context);
                }
                else if (!path.Value.StartsWith("/Auth/") && !path.Value.StartsWith("/Support/"))
                {
                    var (status, response, user) = await ValidationService.ValidationUser(context);
                    if (user == null || string.IsNullOrEmpty(user.Id))
                    {
                        context.Response.StatusCode = status;
                        var buffer = Encoding.UTF8.GetBytes(response);
                        await context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        context.Items["user"] = user;
                        context.Items["userId"] = user.Id;
                        await _next(context);
                    }
                }
                else if (path.Value.StartsWith("/Support/"))
                {
                    Claim? claimId = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid);
                    if (claimId != null && !string.IsNullOrEmpty(claimId.Value))
                    {
                        context.Items["userId"] = claimId.Value;
                        await _next(context);
                    }
                    else
                    {
                        context.Response.StatusCode = 401;
                        var buffer = Encoding.UTF8.GetBytes("User not authorize!");
                        await context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
                else await _next(context);
            }
        }
    }
}
