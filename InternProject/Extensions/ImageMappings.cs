using InternProject.Models.ImageModels;

namespace InternProject.Extensions
{
    public static class ImageMappings
    {
        public static Images ToModel(string imageUrl, Guid ownerId, ImageOwnerType ownerType)
        {
            return new Images
            {
                OwnerId = ownerId,
                ImageUrl = imageUrl,
                ImageOwnerType = ownerType
            };
        }
    }
}
