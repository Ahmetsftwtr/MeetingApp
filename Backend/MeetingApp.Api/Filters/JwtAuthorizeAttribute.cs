using MeetingApp.Business.Abstractions.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.Api.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class JwtAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var services = context.HttpContext.RequestServices;
            var jwtService = services.GetRequiredService<IJwtService>();

            var token = ExtractToken(context.HttpContext);
            if (string.IsNullOrWhiteSpace(token))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    success = false,
                    message = "Token bulunamadý. Lütfen giriþ yapýn."
                });
                return;
            }

            var principal = jwtService.ValidateToken(token);
            if (principal is null)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    success = false,
                    message = "Geçersiz veya süresi dolmuþ token."
                });
                return;
            }

            context.HttpContext.User = principal;

            await Task.CompletedTask;
        }

        private static string? ExtractToken(HttpContext context)
        {
            var header = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(header) && header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return header.Substring("Bearer ".Length).Trim();
            return null;
        }
    }
}