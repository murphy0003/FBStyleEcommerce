
using InternProject.Data;
using InternProject.Models.ImageModels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace InternProject.Services.ImageService
{
    public class ImageBackgroundWorker(IServiceScopeFactory serviceScopeFactory,ImageQueue imageQueue) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const int batchSize = 8;

            var buffer = new List<ImageJob>(batchSize);

            await foreach (var job in imageQueue.DequeueAsync(stoppingToken))
            {
                buffer.Add(job);

                if (buffer.Count < batchSize)
                    continue;

                await ProcessBatchAsync(buffer, stoppingToken);
                buffer.Clear();
            }
        }
        private async Task ProcessBatchAsync(List<ImageJob> batch,CancellationToken stoppingToken)
        {
            await Parallel.ForEachAsync(
               batch,
               new ParallelOptions
               {
                   MaxDegreeOfParallelism = 4,
                   CancellationToken = stoppingToken
               },
               async (job, token) =>
               {
                   await ProcessImageAsync(job, token);
               });

        }
        private async Task ProcessImageAsync(ImageJob job, CancellationToken stoppingToken)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var images = await db.Images.FindAsync([job.ImageId], stoppingToken);
            if (images == null) return;

            images.Status = ImageStatus.Processing;
            await db.SaveChangesAsync(stoppingToken);

            try
            {
                var base64 = job.Base64Data.Contains(',')
                    ? job.Base64Data.Split(',')[1]
                    : job.Base64Data;

                byte[] bytes = Convert.FromBase64String(base64);

                string uploads = Path.Combine("wwwroot", "uploads");
                Directory.CreateDirectory(uploads);

                string fileName = $"{images.ImageId}.webp";
                string path = Path.Combine(uploads, fileName);

                using var img = Image.Load(bytes);
                img.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(1280, 1280)
                }));

                await img.SaveAsWebpAsync(path, stoppingToken);

                images.ImageUrl = $"/uploads/{fileName}";
                images.Status = ImageStatus.Completed;
            }
            catch
            {
                images.Status = ImageStatus.Failed;
            }

            await db.SaveChangesAsync(stoppingToken);
        }
    }
}
