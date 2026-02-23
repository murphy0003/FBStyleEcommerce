using InternProject.Dtos;
using InternProject.Models.OrderModels;
using InternProject.Services.OrderService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InternProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(IOrderService orderService) : ControllerBase
    {
        [HttpPost]
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
        public async Task<ActionResult> GetOrders(
                [FromQuery] bool? isRead,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10)
        {
            var result = await orderService.GetOrdersAsync(
                isRead,
                pageNumber,
                pageSize);
            HttpContext.Items["ResponseMessage"] = "Orders retrieved successfully";
            Response.Headers.CacheControl = "no-cache";
            return Ok(result);
        }
    }
}
