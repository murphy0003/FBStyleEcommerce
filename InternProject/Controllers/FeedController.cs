using InternProject.Extensions;
using InternProject.Services.FeedService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace InternProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedController(IFeedService feedService) : ControllerBase
    {
        [HttpGet("global")]
        [OutputCache(Duration = 10,
        VaryByQueryKeys = new[] { "cursor", "itemName", "pageSize" })]
        public async Task<IActionResult> GetGlobalFeed(
                [FromQuery] string? itemName,
                [FromQuery] string? cursor,
                CancellationToken ct,
                [FromQuery] int pageSize = 20)
        {
            var feedSvcType = feedService.GetType();

            var targetMethod = feedSvcType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(m =>
                {
                    if (m.Name != "GetGlobalFeed") return false;
                    var parameters = m.GetParameters();
                    if (parameters.Length < 2) return false;
                    var paramType = parameters[1].ParameterType;
                    return paramType.IsGenericType && paramType.GetGenericTypeDefinition().Name.StartsWith("CompositeCursor`");
                }) ?? throw new InvalidOperationException(
                    "Unable to determine the generic type arguments for the cursor parameter of IFeedService.GetGlobalFeed. " +
                    "Please either (A) call CursorEncoder.Decode with explicit generic type arguments, e.g. " +
                    "CursorEncoder.Decode<long, Guid>(cursor), or (B) provide the IFeedService.GetGlobalFeed signature so the controller can be adjusted.");
            var cursorParamType = targetMethod.GetParameters()[1].ParameterType;
            var genericArgs = cursorParamType.GetGenericArguments();

            var decodeMethodDef = typeof(CursorEncoder)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "Decode" && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2) ?? throw new InvalidOperationException("CursorEncoder.Decode<TPrimary,TSecondary> not found.");
            var decodeClosed = decodeMethodDef.MakeGenericMethod(genericArgs);

            var compositeCursor = decodeClosed.Invoke(null, [cursor]);

            dynamic dynamicService = feedService;
            var result = await dynamicService.GetGlobalFeed(itemName, compositeCursor, ct, pageSize);

            var response = new
            {
                result.ItemsData,
                Pagination = new
                {
                    NextCursor = CursorEncoder.Encode(result.Pagination.Cursor),
                    result.Pagination.HasMore
                }
            };

            return Ok(response);
        }
    }
}
