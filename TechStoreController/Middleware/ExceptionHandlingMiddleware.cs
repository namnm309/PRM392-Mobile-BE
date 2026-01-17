using BAL.DTOs.Common;
using System.Net;
using System.Text.Json;

namespace TechStoreController.Middleware
{
    /// <summary>
    /// Global exception handling middleware with error codes
    /// </summary>
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
                await _next(context);
            }
            catch (Exception ex)
            {
                var requestId = context.Items["RequestId"]?.ToString() ?? "Unknown";
                _logger.LogError(ex, "An unhandled exception occurred. RequestId: {RequestId}", requestId);
                await HandleExceptionAsync(context, ex, requestId);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, string requestId)
        {
            context.Response.ContentType = "application/json";
            
            var (statusCode, errorCode, message) = MapException(exception);
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                success = false,
                message = message,
                errorCode = errorCode,
                requestId = requestId,
                errors = new List<string> { exception.Message }
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }

        private static (HttpStatusCode statusCode, string errorCode, string message) MapException(Exception exception)
        {
            return exception switch
            {
                InvalidOperationException => (HttpStatusCode.BadRequest, "INVALID_OPERATION", exception.Message),
                ArgumentException => (HttpStatusCode.BadRequest, "INVALID_ARGUMENT", exception.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", "Unauthorized access"),
                KeyNotFoundException => (HttpStatusCode.NotFound, "NOT_FOUND", exception.Message),
                _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred")
            };
        }
    }
}
