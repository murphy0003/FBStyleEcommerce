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

            builder.Property(e => e.ImageId)
                   .HasDefaultValueSql("NEWSEQUENTIALID()");

           
            builder.HasOne(e => e.Post)
                   .WithMany(p => p.Images)
                   .HasForeignKey(e => e.PostId)
                   .OnDelete(DeleteBehavior.Cascade);

            
            builder.HasOne(e => e.Profile)
                   .WithMany(p => p.Images)
                   .HasForeignKey(e => e.ProfileId)
                   .OnDelete(DeleteBehavior.NoAction);

            
            builder.HasIndex(e => e.PostId);
            builder.HasIndex(e => e.ProfileId);
        }
    }
}
