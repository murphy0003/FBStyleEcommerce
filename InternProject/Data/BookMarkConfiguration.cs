using InternProject.Models.BookMarkModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternProject.Data
{
    public class BookMarkConfiguration : IEntityTypeConfiguration<BookMark>
    {
        public void Configure(EntityTypeBuilder<BookMark> builder)
        {
           
            builder.HasKey(bm => new { bm.UserId, bm.PostId });

           
            builder.HasOne(bm => bm.User)
                   .WithMany()
                   .HasForeignKey(bm => bm.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

           
            builder.HasOne(bm => bm.Post)
                   .WithMany() 
                   .HasForeignKey(bm => bm.PostId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(bm => bm.SavedAt).IsRequired();
        }
    }
}
