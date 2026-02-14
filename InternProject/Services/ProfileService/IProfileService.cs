using InternProject.Dtos;

namespace InternProject.Services.ProfileService
{
    public interface IProfileService
    {
        Task<ProfileResponseDto> GetOrCreateProfile(CancellationToken ct);
        Task<ProfileResponseDto> UpdateProfile(Guid profileId, UpdateProfileRequestDto request, CancellationToken ct);
    }
}
