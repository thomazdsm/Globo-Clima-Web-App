using GloboClima.WebApp.Services;

namespace GloboClima.WebApp.Middleware
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
        {
            // Skip token validation for auth endpoints and static files
            var path = context.Request.Path.Value?.ToLower();
            if (path != null && (path.StartsWith("/auth") ||
                                path.StartsWith("/home") ||
                                path.StartsWith("/css") ||
                                path.StartsWith("/js") ||
                                path.StartsWith("/lib")))
            {
                await _next(context);
                return;
            }

            // Only validate tokens for authenticated users
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var tokenRefreshed = await tokenService.RefreshTokenIfNeededAsync();

                if (!tokenRefreshed)
                {
                    // Token refresh failed, redirect to login
                    context.Response.Redirect("/Auth/Login");
                    return;
                }
            }

            await _next(context);
        }
    }
}
