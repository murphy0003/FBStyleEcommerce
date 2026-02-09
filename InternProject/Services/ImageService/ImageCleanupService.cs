using InternProject.Models.ImageModels;

namespace InternProject.Services.ImageService
{
    public class ImageCleanupService : IImageCleanupService
    {
        public Task DeleteImagesAsync(IEnumerable<Images> images, CancellationToken cancellationToken)
        {
            foreach (var image in images)
            {
                if (string.IsNullOrWhiteSpace(image.ImageUrl))
                {
                    continue;
                }
                var filePath = Path.Combine("wwwroot", image.ImageUrl.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                
            }
            return Task.CompletedTask;
        }
    }
}
