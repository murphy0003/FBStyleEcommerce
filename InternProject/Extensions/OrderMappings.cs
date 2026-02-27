using InternProject.Dtos;
using InternProject.Models.ImageModels;
using InternProject.Models.OrderModels;
using System.Linq.Expressions;

namespace InternProject.Extensions
{
    public static class OrderMappings
    {
        public static Order ToModel(OrderCreateDto orderCreateDto, Guid profileId, string PhoneNumber,string? address,string? postimageUrl, string itemName ,decimal price)
        {
            return new Order
            {
                PostId = orderCreateDto.PostId,
                ProfileId = profileId,
                ItemName = itemName,
                Price = price,
                CustomerPhoneNumber = !string.IsNullOrWhiteSpace(orderCreateDto.BuyerPhoneNumber) ? orderCreateDto.BuyerPhoneNumber : PhoneNumber,
                ShippingAddress = !string.IsNullOrWhiteSpace(orderCreateDto.ShippingAddress) ? orderCreateDto.ShippingAddress : address,
                PostImageUrl = postimageUrl,
                IsRead = false,
                OrderStatus = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
        }
        public static OrderResponseV1Dto ToResponseDto(Order order)
        {
            return new OrderResponseV1Dto(
                order.OrderId,
                order.ItemName,
                order.OrderStatus.ToString(),
                order.CreatedAt,
                order.PostImageUrl ?? ""
            );
        }
        public static Expression<Func<Order, OrderResponseV2Dto>> ToResponseV2Dto =>
        order => new OrderResponseV2Dto(
            new CustomerProfile(
                order.Profile.ProfileId,
                order.Profile.DisplayName,
                order.Profile.Images
                    .Where(img => img.ImageOwnerType == ImageOwnerType.Profile && img.Status == ImageStatus.Completed)
                    .OrderByDescending(img => img.CreatedAt)
                    .Select(img => img.ImageUrl)
                    .FirstOrDefault() ?? string.Empty
            ),
            new CustomerOrder(
                order.OrderId,
                order.ItemName,
                order.Price,
                order.OrderStatus.ToString(),
                order.IsRead,
                order.CustomerPhoneNumber!,
                order.ShippingAddress!,
                order.PostImageUrl!
        ));

    }
}
