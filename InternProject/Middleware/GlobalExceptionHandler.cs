using InternProject.Models;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace InternProject.Middleware
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            httpContext.Response.ContentType = "application/json";
            ApiResponse<object> response;
            if(exception is ApiException apiException)
            {
                httpContext.Response.StatusCode = apiException.StatusCode;
                response = new ApiResponse<object>(
                    false,
                    "Request Failed",
                    new
                    {
                        code = apiException.Code,
                        detail = apiException.Details
                    },
                    new
                    {
                        endpoint = httpContext.Request.Path.Value,
                        timeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    });
            }
            else
            {
                _logger.LogError(exception, "Unhandled exception");
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = new ApiResponse<object>(
                    false,
                    "A server error occurred. Please try again later.",
                    null!,
                    new
                    {
                        endpoint = httpContext.Request.Path.Value,
                        timeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    });
            }
            await httpContext.Response.WriteAsJsonAsync(response , cancellationToken);
            return true;
        }
    }
}
