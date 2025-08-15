using NLog;

namespace StudentManagementSystem.API.Middlewares
{
    public class StudentMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public StudentMiddleware(RequestDelegate next)
        {
            _next = next;
            _logger = (Microsoft.Extensions.Logging.ILogger?)LogManager.GetCurrentClassLogger();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            var method = context.Request.Method.ToUpper();

            // Check for IsReadOnly header
            if ((method == "POST" || method == "PUT") && path?.Contains("student") == true)
            {
                if (context.Request.Headers.TryGetValue("IsReadOnly", out var isReadOnlyValues))
                {
                    if (bool.TryParse(isReadOnlyValues.FirstOrDefault(), out var isReadOnly) && isReadOnly)
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("Read-only mode is enabled. Cannot perform write operations.");
                        return;
                    }
                }

                // Log create and update operations
                var operation = method == "POST" ? "Create" : "Update";
                _logger.LogInformation($"Student {operation} operation requested at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            }

            await _next(context);
        }
    }
}