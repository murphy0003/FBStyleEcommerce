using InternProject.Models.AddressModels;
using InternProject.Models.ImageModels;
using InternProject.Models.UserModels;
using System.Collections;

namespace InternProject.Models.ProfileModels
{
    public class Profile
    {
        public Guid ProfileId { get; set; }
        public required string DisplayName { get; set; }
        public required string PhoneNumber { get; set; }
        public bool BlueMark { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public ICollection <Images> Images { get; set; } = new HashSet<Images>();
        public ICollection<SocialAddress> SocialAddresses { get; set; } = new HashSet<SocialAddress>();
    }
}
