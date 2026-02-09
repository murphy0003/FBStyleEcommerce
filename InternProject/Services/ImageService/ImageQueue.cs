using System.Threading.Channels;

namespace InternProject.Services.ImageService
{
    public class ImageQueue
    {
        private readonly Channel<ImageJob> _queue =
        Channel.CreateUnbounded<ImageJob>();

        public void Enqueue(Guid imageId, string base64)
            => _queue.Writer.TryWrite(new ImageJob(imageId, base64));
        

        public IAsyncEnumerable<ImageJob> DequeueAsync(CancellationToken ct)
            => _queue.Reader.ReadAllAsync(ct);
    }
}
