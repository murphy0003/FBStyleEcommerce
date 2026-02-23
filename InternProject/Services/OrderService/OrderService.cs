using InternProject.Data;
using InternProject.Dtos;
using InternProject.Extensions;
using InternProject.Models.ApiModels;
using InternProject.Models.OrderModels;
using InternProject.Models.PagingModels;
using InternProject.Services.UserService;
using Microsoft.EntityFrameworkCore;

namespace InternProject.Services.OrderService
{
    public class OrderService(AppDbContext appDbContext,IUserContext userContext) : IOrderService
    {
        public async Task <OrderResponseV1Dto> CreateOrderAsync(
                OrderCreateDto orderCreateDto,
                CancellationToken cancellationToken)
        {
            var userId = userContext.GetCurrentUserId();

            var post = await appDbContext.Posts
                .Include(p => p.Images)
                .FirstOrDefaultAsync(
                    p => p.PostId == orderCreateDto.PostId,
                    cancellationToken)
                ?? throw new ApiException(
                    "Post not found.",
                    null,
                    StatusCodes.Status404NotFound);

            var userProfile = await appDbContext.Profiles
                .FirstOrDefaultAsync(
                    p => p.UserId == userId,
                    cancellationToken)
                ?? throw new ApiException(
                    "User profile not found.",
                    null,
                    StatusCodes.Status404NotFound);

            var defaultAddress = await appDbContext.Addresses
                .FirstOrDefaultAsync(
                    a => a.UserId == userId && a.IsDefault,
                    cancellationToken);

            string? finalPhone =
                !string.IsNullOrWhiteSpace(orderCreateDto.BuyerPhoneNumber)
                    ? orderCreateDto.BuyerPhoneNumber
                    : userProfile.PhoneNumber;

            if (string.IsNullOrWhiteSpace(finalPhone))
                throw new ApiException(
                    "A contact phone number is required to place an order.",
                    null,
                    StatusCodes.Status400BadRequest);

            string? finalAddress =
                !string.IsNullOrWhiteSpace(orderCreateDto.ShippingAddress)
                    ? orderCreateDto.ShippingAddress
                    : defaultAddress?.AddressName;

            if (string.IsNullOrWhiteSpace(finalAddress))
                throw new ApiException(
                    "A shipping address is required. Please provide one or set a default in your profile.",
                    null,
                    StatusCodes.Status400BadRequest);

            var firstImageUrl = post.Images?
                .FirstOrDefault()?
                .ImageUrl;

            var newOrder = OrderMappings.ToModel(
                orderCreateDto,
                userProfile.ProfileId,
                finalPhone,
                finalAddress,
                firstImageUrl,
                post.ItemName,
                post.Price
            );

            appDbContext.Orders.Add(newOrder);
            await appDbContext.SaveChangesAsync(cancellationToken);
            return OrderMappings.ToResponseDto(newOrder);
        }
        public async Task<OrderResponseV1Dto> UpdateOrderStatusAsync(
                    Guid orderId,
                    OrderStatus newStatus,
                    CancellationToken ct)
        {
            var currentUserId = userContext.GetCurrentUserId();

            var order = await appDbContext.Orders
                .Include(o => o.Profile)
                .FirstOrDefaultAsync(
                    o => o.OrderId == orderId &&
                         o.Profile.UserId == currentUserId,
                    ct)
                ?? throw new ApiException(
                    "Order not found or access denied.",
                    null,
                    StatusCodes.Status404NotFound);

            if (order.OrderStatus == OrderStatus.Canceled ||
                order.OrderStatus == OrderStatus.Rejected)
                throw new ApiException(
                    "This order can no longer be modified.",
                    null,
                    StatusCodes.Status400BadRequest);

            if (order.OrderStatus == newStatus)
                throw new ApiException(
                    "Order is already in the requested status.",
                    null,
                    StatusCodes.Status400BadRequest);

            if (newStatus == OrderStatus.Canceled)
            {
                if (order.OrderStatus != OrderStatus.Pending)
                    throw new ApiException(
                        "Only pending orders can be canceled.",
                        null,
                        StatusCodes.Status400BadRequest);

                var hoursPassed =
                    (DateTime.UtcNow - order.CreatedAt).TotalHours;

                if (hoursPassed > 24)
                    throw new ApiException(
                        "Order cannot be canceled after 24 hours.",
                        null,
                        StatusCodes.Status400BadRequest);
            }

            order.OrderStatus = newStatus;

            await appDbContext.SaveChangesAsync(ct);

            return OrderMappings.ToResponseDto(order);
        }
        public async Task UpdateReadStatusAsync(
                Guid orderId,
                bool status,
                CancellationToken ct)
        {
            var currentUserId = userContext.GetCurrentUserId();

            var order = await appDbContext.Orders
                .Include(o => o.Profile)
                .FirstOrDefaultAsync(
                    o => o.OrderId == orderId &&
                         o.Profile.UserId == currentUserId,
                    ct)
                ?? throw new ApiException(
                    "Order not found or access denied.",
                    null,
                    StatusCodes.Status404NotFound);

            if (order.IsRead == status)
                return;

            order.IsRead = status;
            order.UpdatedAt = DateTime.UtcNow;

            await appDbContext.SaveChangesAsync(ct);
        }
        public async Task<PaginationModel<OrderResponseV2Dto>> GetOrdersAsync(bool? isRead,int pageNumber,int pageSize)
        {
            var currentUserId = userContext.GetCurrentUserId();

            var query = appDbContext.Orders
                    .AsNoTracking()
                    .Where(o => o.Profile.UserId == currentUserId);

            if (isRead.HasValue)
                    query = query.Where(o => o.IsRead == isRead.Value);

            return await query
                    .OrderByDescending(o => o.CreatedAt)
                    .Select(OrderMappings.ToResponseV2Dto) 
                    .ToPaginatedListAsync(pageNumber, pageSize);
        }
    }
}
