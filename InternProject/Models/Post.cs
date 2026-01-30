namespace InternProject.Models
{
    public class Post
    {
        public Guid PostId { get; set; } = Guid.CreateVersion7();
        public required string ItemName { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public BuyerCondition BuyerCondition { get; set; }
        public ItemCondition ItemCondition { get; set; }
        public ItemStatus ItemStatus { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid SellerId { get; set; }
        public User Seller {  get; set; }


    }
}
