namespace InternProject.Dtos
{
    public record BookMarkPageDto(
        Guid PostId,
        string ItemName,
        decimal Price,
        string ImageUrl,
        DateTime CreatedAt,
        bool IsSaved
    );
}
