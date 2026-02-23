namespace InternProject.Dtos
{
   public record OrderCreateDto(
        Guid PostId,
        string? BuyerPhoneNumber,
        string? ShippingAddress
    );
    public record OrderResponseV1Dto(
        Guid OrderId,
        string ItemName,
        string OrderStatus,
        DateTime CreatedAt,
        string PostImageUrl
    );
    public record OrderResponseV2Dto
    (
        CustomerProfile CustomerProfile, CustomerOrder CustomerOrder
    );
    public record CustomerProfile(
        Guid ProfileId,
        string DisplayName,
        string ProfileImageUrl
    );
    public record CustomerOrder(
        Guid OrderId,
        string ItemName,
        Decimal Price,
        string OrderStatus,
        bool IsRead,
        string CustomerPhoneNumber,
        string ShippingAddress,
        string PostImageUrl
    );
}
