using InternProject.Dtos;
using InternProject.Models.OrderModels;
using InternProject.Services.OrderService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InternProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(IOrderService orderService) : ControllerBase
    {
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateOrder(
                    [FromBody] OrderCreateDto orderCreateDto,
                    CancellationToken cancellationToken)
        {
            var result = await orderService.CreateOrderAsync(
                orderCreateDto,
                cancellationToken);

            HttpContext.Items["ResponseMessage"] = "Order created successfully";
            Response.Headers.CacheControl = "no-cache";

            return StatusCode(StatusCodes.Status201Created, result);
        }
        [HttpPatch("{orderId:guid}/status")]
        [Authorize]
        public async Task<ActionResult> UpdateOrderStatus(
                Guid orderId,
                [FromBody] OrderStatus status,
                CancellationToken cancellationToken)
        {
            var result = await orderService.UpdateOrderStatusAsync(
                orderId,
                status,
                cancellationToken);

            HttpContext.Items["ResponseMessage"] = "Order status updated successfully";
            Response.Headers.CacheControl = "no-cache";

            return Ok(result);
        }
        [HttpPatch("{orderId:guid}/read-status")]
        [Authorize]
        public async Task<ActionResult> UpdateReadStatus(
                Guid orderId,
                [FromQuery] bool status,
                CancellationToken cancellationToken)
        {
            await orderService.UpdateReadStatusAsync(
                orderId,
                status,
                cancellationToken);

            HttpContext.Items["ResponseMessage"] = "Order read status updated";
            Response.Headers.CacheControl = "no-cache";

            return Ok();
        }
        [HttpGet("unread")]
        [Authorize]
        public async Task<ActionResult> GetOrders(
                [FromQuery] bool? isRead,
                CancellationToken ct,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10
                )
        {
            var result = await orderService.GetOrdersAsync(
                isRead,
                pageNumber,
                pageSize, ct);
            HttpContext.Items["ResponseMessage"] = "Orders retrieved successfully";
            Response.Headers.CacheControl = "no-cache";
            return Ok(result);
        }
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetOrders(
                [FromQuery] OrderStatus? status,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                CancellationToken cancellationToken = default)
        {
            var result = await orderService.GetOrdersAsync(
                status,
                pageNumber,
                pageSize,
                cancellationToken);

            HttpContext.Items["ResponseMessage"] = "Orders retrieved successfully";
            Response.Headers.CacheControl = "no-cache";

            return Ok(result);
        }
        [HttpGet("statistics")]
        [Authorize]
        public async Task<ActionResult> GetOrderStatistics(
                CancellationToken cancellationToken)
        {
            var result = await orderService.GetOrderStatisticsAsync(cancellationToken);
            HttpContext.Items["ResponseMessage"] = "Order statistics retrieved successfully";
            Response.Headers.CacheControl = "no-cache";
            return Ok(result);

        }
    }
}
