using InternProject.Models.BookMarkModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternProject.Data
{
    public class BookMarkConfiguration : IEntityTypeConfiguration<BookMark>
    {
        public void Configure(EntityTypeBuilder<BookMark> builder)
        {
            builder.HasKey(bm => new {bm.UserId,bm.PostId});
        }
    }
}
