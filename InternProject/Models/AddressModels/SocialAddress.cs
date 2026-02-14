using InternProject.Models.ProfileModels;
using InternProject.Models.UserModels;

namespace InternProject.Models.AddressModels
{
    public class SocialAddress
    {
        public Guid SocialAddressId { get; set; }
        public required string SocialLink { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid ProfileId { get; set; }
        public virtual Profile Profile { get; set; } = null!;
    }
}
