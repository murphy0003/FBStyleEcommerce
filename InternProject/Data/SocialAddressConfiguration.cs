using InternProject.Models.AddressModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternProject.Data
{
    public class SocialAddressConfiguration : IEntityTypeConfiguration<SocialAddress>
    {
        public void Configure(EntityTypeBuilder<SocialAddress> builder)
        {
            builder.HasKey(a => a.SocialAddressId);
            builder.Property(a => a.SocialAddressId).HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.HasOne(a => a.Profile)
                   .WithMany(p => p.SocialAddresses)
                   .HasForeignKey(a => a.ProfileId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
