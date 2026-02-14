using InternProject.Models.UserModels;

namespace InternProject.Models.AddressModels
{
    public class Address
    {
        public Guid AddressId { get; set; }
        public required string AddressName { get; set; }
        public string DeliveryInstructions { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDefault { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
