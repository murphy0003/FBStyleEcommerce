using InternProject.Models.PostModels;
using InternProject.Models.ProfileModels;

namespace InternProject.Models.ImageModels
{
    public class Images
    {
        public Guid ImageId { get; set; }
        public Guid? PostId { get; set; }
        public Post? Post { get; set; }
        public Guid? ProfileId { get; set; }
        public Profile? Profile { get; set; }
        public required string ImageUrl { get; set; }
        public ImageStatus Status { get; set; } = ImageStatus.Pending;
        public DateTime CreatedAt { get; set; } =DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }
}
