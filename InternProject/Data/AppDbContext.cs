using InternProject.Models.ImageModels;
using InternProject.Models.PostModels;
using InternProject.Models.UserModels;
using Microsoft.EntityFrameworkCore;

namespace InternProject.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options):DbContext (options)
    {
        public DbSet<Users> Users => Set<Users>();
        public DbSet<Posts> Posts => Set<Posts>();
        public DbSet<Images> Images => Set<Images>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.HasIndex(u => u.Email)
                      .IsUnique();
                entity.Property(u => u.UserId)
                      .HasDefaultValueSql("NEWSEQUENTIALID()");
            });
            modelBuilder.Entity<Posts>(entity =>
            {
                entity.HasKey(u => u.PostId);
                entity.Property(p => p.PostId)
                      .HasDefaultValueSql("NEWSEQUENTIALID()");
                entity.HasOne(p => p.Seller)
                      .WithMany(u => u.Posts)
                      .HasForeignKey(p => p.SellerId)
                      .OnDelete(DeleteBehavior.Cascade);
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

        }
    }
}
