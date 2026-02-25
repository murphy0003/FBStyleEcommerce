using InternProject.Dtos;
using InternProject.Models.OrderModels;
using InternProject.Models.PagingModels;

namespace InternProject.Services.OrderService
{
    public interface IOrderService
    {
        Task <OrderResponseV1Dto> CreateOrderAsync(OrderCreateDto orderCreateDto , CancellationToken cancellationToken);
        Task <OrderResponseV1Dto> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, CancellationToken cancellationToken);
        Task UpdateReadStatusAsync(Guid orderId, bool status, CancellationToken cancellationToken);
        Task<PaginationModel<OrderResponseV2Dto>> GetOrdersAsync(bool? isRead, int pageNumber, int pageSize, CancellationToken ct);
        Task <PaginationModel<OrderResponseV2Dto>>GetOrdersAsync(OrderStatus? status, int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<OrderStatisticsDto> GetOrderStatisticsAsync(CancellationToken cancellationToken);
    }
}
