using InternProject.Models.ProfileModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternProject.Data
{
    public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
    {
        public void Configure(EntityTypeBuilder<Profile> builder)
        {
            builder.HasKey(p => p.ProfileId);
            builder.Property(p => p.ProfileId).HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.HasOne(p => p.User)
                   .WithOne(u => u.Profile)
                   .HasForeignKey<Profile>(p => p.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(p => p.Images)
                   .WithOne(i=>i.Profile)
                   .HasForeignKey(i => i.ProfileId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
