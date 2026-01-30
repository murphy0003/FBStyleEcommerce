namespace InternProject.Models
{
    public class Image
    {
        public Guid ImageId { get; set; } = Guid.CreateVersion7();
        public Guid OwnerId { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImagePath { get; set; }
        public ImageOwnerType ImageOwnerType { get; set; }
    }
}
