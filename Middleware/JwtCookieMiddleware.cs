using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Airbnb_Clone_Api.Middleware
{
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtCookieMiddleware> _logger;

        public JwtCookieMiddleware(RequestDelegate next, ILogger<JwtCookieMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue("jwt", out var token))
            {
                _logger.LogInformation("JWT found in cookie: {Token}", token);

                // Fix: Set header instead of appending
                context.Request.Headers["Authorization"] = $"Bearer {token}";

                _logger.LogInformation("JWT successfully set in Authorization header.");
            }
            else
            {
                _logger.LogWarning("No JWT found in cookies.");
            }

            await _next(context);
        }

    }
}
