using InternProject.Models.ImageModels;
using InternProject.Models.PostModels;
using InternProject.Models.ProfileModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternProject.Data
{
    public class ImageConfiguration : IEntityTypeConfiguration<Images>
    {
        public void Configure(EntityTypeBuilder<Images> builder)
        {
            builder.HasKey(e => e.ImageId);

            builder.Property(e => e.ImageId)
                   .HasDefaultValueSql("NEWSEQUENTIALID()");

            // 🔹 Relationship to Post
            builder.HasOne(e => e.Post)
                   .WithMany(p => p.Images)
                   .HasForeignKey(e => e.PostId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Relationship to Profile
            builder.HasOne(e => e.Profile)
                   .WithMany(p => p.Images)
                   .HasForeignKey(e => e.ProfileId)
                   .OnDelete(DeleteBehavior.NoAction);

            // Optional: Ensure at least one FK is indexed
            builder.HasIndex(e => e.PostId);
            builder.HasIndex(e => e.ProfileId);
        }
    }
}
