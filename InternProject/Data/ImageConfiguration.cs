using InternProject.Models.ImageModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternProject.Data
{
    public class ImageConfiguration : IEntityTypeConfiguration<Images>
    {
        public void Configure(EntityTypeBuilder<Images> builder)
        {
            builder.HasKey(e => e.ImageId);
            builder.Property(e => e.ImageId).HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(e => e.OwnerId).IsRequired();

            builder.HasIndex(e => new { e.OwnerId, e.ImageOwnerType , e.CreatedAt })
                   .IsDescending()
                   .HasDatabaseName("IX_Images_OwnerId_OwnerType")
                   .IncludeProperties(img => img.ImageUrl);
        }
    }
}
