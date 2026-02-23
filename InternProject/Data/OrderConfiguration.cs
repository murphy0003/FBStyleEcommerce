using InternProject.Models.OrderModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternProject.Data
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.OrderId);
            builder.Property(o => o.OrderId).HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.Property(o => o.Price).HasPrecision(18, 2);
            builder.HasOne(o => o.Post)
                   .WithMany()
                   .HasForeignKey(o => o.PostId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Profile)
                   .WithMany()
                   .HasForeignKey(o => o.ProfileId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
