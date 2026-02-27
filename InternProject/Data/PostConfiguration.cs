using InternProject.Models.PostModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternProject.Data
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.HasKey(p => p.PostId);
            builder.Property(p => p.PostId).HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.Property(p => p.Price).HasPrecision(18, 2);

            builder.HasIndex(p => new { p.CreatedAt, p.PostId })
                    .IsDescending() 
                    .HasDatabaseName("IX_Posts_Feed_Pagination")
                    .IncludeProperties(p => p.ItemName);
            builder.HasOne(p => p.Seller)
                   .WithMany(u => u.Posts)
                   .HasForeignKey(p => p.SellerId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(p => p.Images)
                   .WithOne()
                   .HasForeignKey(i => i.OwnerId)
                   .HasPrincipalKey(p=>p.PostId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
