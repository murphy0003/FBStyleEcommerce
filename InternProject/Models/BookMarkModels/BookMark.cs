using InternProject.Models.PostModels;
using InternProject.Models.UserModels;

namespace InternProject.Models.BookMarkModels
{
    public class BookMark
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid PostId { get; set; }
        public Post Post { get; set; } = null!;

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}
