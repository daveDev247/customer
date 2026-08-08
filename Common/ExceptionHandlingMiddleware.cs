using System.Net;
using System.Text.Json;

namespace CustomerApi.Common
{
    // Catches anything unhandled by controllers/services, logs it with a correlation ID,
    // and returns the same ApiResponse<T> shape as every other endpoint — so callers never
    // see a raw stack trace or an inconsistent error format.
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Let the request flow through normally; only step in if something throws.
                await _next(context);
            }
            catch (Exception ex)
            {
                // TraceIdentifier is ASP.NET Core's built-in per-request correlation ID —
                // this is what ties a user's error message back to exact log lines.
                var correlationId = context.TraceIdentifier;

                _logger.LogError(ex,
                    "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}",
                    correlationId, context.Request.Path);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = ApiResponse<object>.FailResponse(
                    "An unexpected error occurred",
                    correlationId: correlationId);

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}
