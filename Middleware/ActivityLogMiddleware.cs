using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Controllers;
using ShopInventory.Data;
using ShopInventory.Models;

namespace ShopInventory.Middleware
{
    public class ActivityLogMiddleware
    {
        private readonly RequestDelegate _next;

        public ActivityLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            // Skip activity logging for anonymous access and authentication-related endpoints
            if (!context.User?.Identity?.IsAuthenticated ?? true ||
                context.Request.Path.StartsWithSegments("/Account"))
            {
                await _next(context);
                return;
            }

            await _next(context); // Always call next only once, then log

            try
            {
                var endpoint = context.GetEndpoint();
                if (endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>() is ControllerActionDescriptor actionDescriptor)
                {
                    var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var log = new ActivityLog
                        {
                            Timestamp = DateTime.UtcNow,
                            Action = actionDescriptor.ActionName,
                            EntityName = actionDescriptor.ControllerName,
                            UserId = userId,
                            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                            Description = $"Accessed {actionDescriptor.ControllerName}/{actionDescriptor.ActionName}"
                        };
                        dbContext.ActivityLogs.Add(log);
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
            catch
            {
                // Swallow all logging errors to avoid breaking the response
            }
        }
    }

    public static class ActivityLogMiddlewareExtensions
    {
        public static IApplicationBuilder UseActivityLog(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ActivityLogMiddleware>();
        }
    }
}