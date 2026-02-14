using InternProject.Models.AddressModels;
using InternProject.Models.ImageModels;
using InternProject.Models.PostModels;
using InternProject.Models.ProfileModels;
using InternProject.Models.UserModels;
using Microsoft.EntityFrameworkCore;

namespace InternProject.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options):DbContext (options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Images> Images => Set<Images>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<SocialAddress> SocialAddresses => Set<SocialAddress>();
        public DbSet<Profile> Profiles => Set<Profile>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.HasIndex(u => u.Email)
                      .IsUnique();
                entity.Property(u => u.UserId)
                      .HasDefaultValueSql("NEWSEQUENTIALID()");
            });
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(u => u.PostId);
                entity.Property(p => p.PostId)
                      .HasDefaultValueSql("NEWSEQUENTIALID()");
                entity.HasOne(p => p.Seller)
                      .WithMany(u => u.Posts)
                      .HasForeignKey(p => p.SellerId)
                      .OnDelete(DeleteBehavior.Cascade);
                modelBuilder.Entity<Post>()
                      .HasIndex(p => new { p.ItemName, p.CreatedAt })
                      .IsDescending(false, true);
            });
            modelBuilder.Entity<Images>(entity =>
            {
                entity.HasKey(e => e.ImageId);
                entity.Property(e => e.ImageId)
                      .HasDefaultValueSql("NEWSEQUENTIALID()"); 
                entity.Property(e => e.OwnerId)
                      .IsRequired();
                entity.HasIndex(e => new { e.OwnerId, e.ImageOwnerType })
                      .HasDatabaseName("IX_Images_OwnerId_OwnerType");
            });
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(a => a.AddressId);
                entity.Property(a => a.AddressId)
                      .HasDefaultValueSql("NEWSEQUENTIALID()");

                entity.HasOne(a => a.User)
                      .WithMany(u => u.Addresses)
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<SocialAddress>(entity =>
            {
                entity.HasKey(a => a.SocialAddressId);
                entity.Property(a => a.SocialAddressId)
                      .HasDefaultValueSql("NEWSEQUENTIALID()");

                entity.HasOne(a => a.Profile)
                      .WithMany(u => u.SocialAddresses)
                      .HasForeignKey(a => a.ProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Profile>(entity =>
            {
                entity.HasKey(p => p.ProfileId);
                entity.Property(p => p.ProfileId)
                      .HasDefaultValueSql("NEWSEQUENTIALID()");
                entity.HasOne(p => p.User)
                      .WithOne(u => u.Profile)
                      .HasForeignKey<Profile>(p => p.UserId);
            });

        }
    }
}
