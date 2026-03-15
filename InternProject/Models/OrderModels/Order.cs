using InternProject.Models.AddressModels;
using InternProject.Models.PostModels;
using InternProject.Models.ProfileModels;

namespace InternProject.Models.OrderModels
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public Decimal Price { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public bool IsRead { get; set; }
        public string? CustomerPhoneNumber { get; set; }
        public string? ShippingAddress { get; set; }
        public string? PostImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid ProfileId { get; set; }
        public Guid PostId { get; set; }
        public virtual Profile Profile { get; set; } = null!;
        public Post Post { get; set; } = null!;

    }
}
