using InternProject.Models.ImageModels;

namespace InternProject.Services.ImageService
{
    public interface IImageCleanupService
    {
        Task DeleteImagesAsync(IEnumerable<Images> images, CancellationToken cancellationToken);
    }
}
