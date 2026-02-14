namespace InternProject.Dtos
{
    public record FeedItemDto ( PostDataDto Postdata , ProfileDataDto ProfileData );
    public record PostDataDto (Guid PostId,string ItemName,Decimal Price,string ItemCondition,string ItemStatus,DateTime CreatedAt,IEnumerable<string> PostImageUrl);
    public record ProfileDataDto (Guid ProfileId ,string DisplayName,string PhoneNumber,bool BlueMark,string? ProfileImageUrl);
}
