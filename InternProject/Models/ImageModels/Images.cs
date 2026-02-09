namespace InternProject.Models.ImageModels
{
    public class Images
    {
        public Guid ImageId { get; set; }
        public Guid OwnerId { get; set; }
        public required string ImageUrl { get; set; }
        public ImageOwnerType ImageOwnerType { get; set; }
        public ImageStatus Status { get; set; } = ImageStatus.Pending;
        public DateTime CreatedAt { get; set; } =DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }
}
