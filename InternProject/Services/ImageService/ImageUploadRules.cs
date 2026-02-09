namespace InternProject.Services.ImageService
{
    public static class ImageUploadRules
    {
        public const int MaxImagesPerPost = 10;
        public const int MaxImageSizeBytes = 5 * 1024 * 1024; // 5MB
    }
}
