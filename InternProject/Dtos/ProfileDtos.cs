namespace InternProject.Dtos
{
    public record ProfileResponseDto(Guid ProfileId, string DisplayName , string PhoneNumber, string? ProfilePictureUrl , bool BlueMark , IEnumerable <string> SocialLink);
    public record UpdateProfileRequestDto(string DisplayName , string PhoneNumber, string ProfilePictureBase64);
    public record OrdersStatisticsData(int TotalOrdered, int TotalSold, int TotalCancelled);
}
