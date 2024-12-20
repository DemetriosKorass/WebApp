namespace WebApp.UI.Middlewares
{
    public class AdminPathMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AdminPathMiddleware> _logger;

        public AdminPathMiddleware(RequestDelegate next, ILogger<AdminPathMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/admin"))
            {
                _logger.LogInformation("Admin path accessed: {Path}", context.Request.Path);
            }

            await _next(context);
        }
    }
}
