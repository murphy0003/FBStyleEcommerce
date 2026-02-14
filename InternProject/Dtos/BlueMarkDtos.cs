namespace InternProject.Dtos
{
    public record BlueMarkCreateDto(string Address , List<string> SocialLinks);
    public record BlueMarkUpdateDto(string Address , List<string> SocialLinks);
    public record BlueMarkResponseDto(bool BlueMark , string Address , List<string> SocialLinks , DateTime CreatedAt);
}
