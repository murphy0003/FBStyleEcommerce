using InternProject.Data;
using InternProject.Dtos;
using InternProject.Models.AddressModels;
using InternProject.Models.ApiModels;
using InternProject.Models.UserModels;
using InternProject.Services.UserService;
using Microsoft.EntityFrameworkCore;

namespace InternProject.Services.BlueMarkService
{
    public class BlueMarkService(AppDbContext context , IUserContext userContext) : IBlueMarkService
    {
        public async Task<BlueMarkResponseDto> CreateBlueMarkAsync(
            BlueMarkCreateDto dto,
            CancellationToken ct = default)
        {
            var userId = userContext.GetCurrentUserId();

            if (userId == Guid.Empty)
                throw new ApiException(
                    "UNAUTHORIZED",
                    null,
                    StatusCodes.Status401Unauthorized
                );

            if (string.IsNullOrWhiteSpace(dto.Address))
                throw new ApiException(
                    "ADDRESS_REQUIRED",
                    null,
                    StatusCodes.Status400BadRequest
                );

            var profile = await context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == userId, ct)
                ?? throw new ApiException(
                    "PROFILE_NOT_FOUND",
                    null,
                    StatusCodes.Status404NotFound
                );

            if (profile.BlueMark)
                throw new ApiException(
                    "BLUE_MARK_ALREADY_EXISTS",
                    null,
                    StatusCodes.Status400BadRequest
                );

            var socialLinksInput = dto.SocialLinks ?? [];

            if (socialLinksInput.Count > 5)
                throw new ApiException(
                    "SOCIAL_LINK_LIMIT_EXCEEDED",
                    new { max = 5 },
                    StatusCodes.Status400BadRequest
                );

            using var transaction = await context.Database.BeginTransactionAsync(ct);

            try
            {
                var address = new Address
                {
                    UserId = userId,
                    AddressName = dto.Address.Trim(),
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow
                };

                var socialLinks = socialLinksInput
                    .Where(link => !string.IsNullOrWhiteSpace(link))
                    .Distinct()
                    .Select(link => new SocialAddress
                    {
                        ProfileId = profile.ProfileId,
                        SocialLink = link.Trim(),
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList();

                profile.BlueMark = true;

                context.Addresses.Add(address);
                context.SocialAddresses.AddRange(socialLinks);

                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return new BlueMarkResponseDto
                (
                    profile.BlueMark,
                    address.AddressName,
                    socialLinks.Select(s => s.SocialLink).ToList()!,
                    address.CreatedAt
                );
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }


        public async Task<BlueMarkResponseDto> UpdateBlueMarkAsync(
            BlueMarkUpdateDto dto,
            CancellationToken ct = default)
        {
            var userId = userContext.GetCurrentUserId();

            if (userId == Guid.Empty)
                throw new ApiException(
                    "UNAUTHORIZED",
                    null,
                    StatusCodes.Status401Unauthorized
                );

            var profile = await context.Profiles
                .Include(p => p.SocialAddresses)
                .FirstOrDefaultAsync(p => p.UserId == userId, ct)
                ?? throw new ApiException(
                    "PROFILE_NOT_FOUND",
                    null,
                    StatusCodes.Status404NotFound
                );

            if (!profile.BlueMark)
                throw new ApiException(
                    "BLUE_MARK_NOT_FOUND",
                    null,
                    StatusCodes.Status400BadRequest
                );

            using var transaction = await context.Database.BeginTransactionAsync(ct);

            try
            {
                var newAddress = dto.Address?.Trim();
                var newSocialLinks = dto.SocialLinks ?? [];
                var addressEntity = await context.Addresses
                    .FirstOrDefaultAsync(a => a.UserId == userId, ct);

                if (addressEntity != null && dto.Address != null)
                {
                    addressEntity.AddressName = newAddress!;
                }
                if (dto.SocialLinks != null)
                {
                    context.SocialAddresses.RemoveRange(profile.SocialAddresses);
                    var cleanedLinks = newSocialLinks
                        .Where(link => !string.IsNullOrWhiteSpace(link))
                        .Distinct()
                        .Select(link => new SocialAddress
                        {
                            ProfileId = profile.ProfileId,
                            SocialLink = link.Trim(),
                            CreatedAt = DateTime.UtcNow
                        })
                        .ToList();

                    await context.SocialAddresses.AddRangeAsync(cleanedLinks, ct);
                }

                await context.SaveChangesAsync(ct);
                var finalAddress = addressEntity?.AddressName;
                var hasAddress = !string.IsNullOrWhiteSpace(finalAddress);

                var socialCount = await context.SocialAddresses
                    .CountAsync(s => s.ProfileId == profile.ProfileId, ct);

                var hasSocialLinks = socialCount > 0;

                if (!hasAddress || !hasSocialLinks)
                {
                    profile.BlueMark = false;
                    await context.SaveChangesAsync(ct);
                }

                await transaction.CommitAsync(ct);

                return new BlueMarkResponseDto
                (
                    profile.BlueMark,
                    finalAddress ?? "",
                    await context.SocialAddresses
                        .Where(s => s.ProfileId == profile.ProfileId )
                        .Select(s => s.SocialLink)
                        .ToListAsync(ct),
                    addressEntity?.CreatedAt ?? DateTime.UtcNow
                );
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

    }
}
