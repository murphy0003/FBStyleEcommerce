using InternProject.Models.ImageModels;

namespace InternProject.Extensions
{
    public static class ImageMappings
    {
        public static Images ToModel(Guid profileId,Guid postId , string imageUrl)
        {
            return new Images
            {
                ProfileId = profileId,
                PostId = postId,
                ImageUrl = imageUrl
            };
        }
    }
}
