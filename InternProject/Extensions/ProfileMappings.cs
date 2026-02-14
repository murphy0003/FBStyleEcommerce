using InternProject.Dtos;
using InternProject.Models.ImageModels;
using InternProject.Models.ProfileModels;

namespace InternProject.Extensions
{
    public static class ProfileMappings
    {
        public static IQueryable<ProfileResponseDto> ProfileToDto(this IQueryable<Profile> query)
        {
            return query.Select(p => new ProfileResponseDto(
                p.ProfileId,
                p.DisplayName,
                p.PhoneNumber,
                p.Images
                .Where(img => img.ImageOwnerType == ImageOwnerType.Profile && img.OwnerId == p.ProfileId)
                .OrderByDescending(img => img.CreatedAt) 
                .Select(img => img.ImageUrl)
                .FirstOrDefault(),
                p.BlueMark,
                p.SocialAddresses.Select(s => s.SocialLink)
            ));
        }
    }
}
