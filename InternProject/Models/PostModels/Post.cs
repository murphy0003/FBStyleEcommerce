using InternProject.Models.ImageModels;
using InternProject.Models.UserModels;

namespace InternProject.Models.PostModels
{
    public class Post
    {
        public Guid PostId { get; set; }
        public required string ItemName { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public BuyerCondition BuyerCondition { get; set; }
        public ItemCondition ItemCondition { get; set; }
        public ItemStatus ItemStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid SellerId { get; set; }
        public virtual User Seller {  get; set; } = null!;
        public virtual ICollection<Images> Images { get; set; } = new HashSet<Images>();
    }
}
