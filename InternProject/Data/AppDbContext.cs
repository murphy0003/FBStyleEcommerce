using InternProject.Models;
using Microsoft.EntityFrameworkCore;

namespace InternProject.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options):DbContext (options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Post> Posts => Set<Post>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<Post>()
                .HasOne(p=>p.Seller)
                .WithMany(u=>u.Posts)
                .HasForeignKey(p=>p.SellerId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Image>(entity =>
            {
                entity.HasKey(e => e.ImageId);
                entity.Property(e => e.OwnerId)
                      .IsRequired();
                entity.HasIndex(e => new { e.OwnerId, e.ImageOwnerType })
                      .HasDatabaseName("IX_Images_OwnerId_OwnerType");
            });

        }
    }
}
