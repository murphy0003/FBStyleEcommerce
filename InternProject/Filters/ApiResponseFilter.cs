using InternProject.Models;
using MailKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InternProject.Filters
{
    public class ApiResponseFilter : IResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is not ObjectResult objectResult)
                return;
            var statusCode = objectResult.StatusCode
                         ?? context.HttpContext.Response.StatusCode;
            if (statusCode < 200 || statusCode >= 300)
                return;
            if (statusCode == StatusCodes.Status204NoContent)
                return;
            if (objectResult.Value?.GetType().IsGenericType == true &&
                objectResult.Value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>))
                return;
            var message = context.HttpContext.Items["ResponseMessage"]?.ToString() ?? "Request Successful";
            var meta = new
            {
                endpoint = context.HttpContext.Request.Path.Value,
                timeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
            context.Result = new ObjectResult(
                new ApiResponse<object>(
                    true,
                    message,
                    objectResult.Value!,
                    meta)
                )
            {
                StatusCode = statusCode
            };
        }
    }
}
