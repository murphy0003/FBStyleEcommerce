using InternProject.Data;
using InternProject.Dtos;
using InternProject.Extensions;
using InternProject.Models.ApiModels;
using InternProject.Models.ImageModels;
using InternProject.Models.ProfileModels;
using InternProject.Models.UserModels;
using InternProject.Services.UserService;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Processing;

namespace InternProject.Services.ProfileService
{
    public class ProfileService(AppDbContext context, IUserContext userContext) : IProfileService
    {
        public async Task<ProfileResponseDto> GetOrCreateProfile(CancellationToken ct)
        {
            var userId = userContext.GetCurrentUserId();

            var existingProfile = await context.Profiles
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .ProfileToDto()
                .FirstOrDefaultAsync(ct);
            if (existingProfile != null)
            {
                return existingProfile;
            }
            var user = await context.Users
                .AsNoTracking()
                .Where(u => u.UserId == userId)
                .Select(u => new { u.UserName, u.PhoneNumber }) 
                .FirstOrDefaultAsync(ct)
                ?? throw new ApiException("User account record not found.",null,StatusCodes.Status400BadRequest);

            var newProfile = new Profile
            {
                UserId = userId,
                DisplayName = user.UserName ?? "New User",
                PhoneNumber = user.PhoneNumber ?? "",
                BlueMark = false,
            };

            context.Profiles.Add(newProfile);
            await context.SaveChangesAsync(ct);
            return new ProfileResponseDto
            (
                newProfile.ProfileId,
                newProfile.DisplayName,
                newProfile.PhoneNumber,
                null,
                newProfile.BlueMark,
                []
            );
        }
        public async Task<ProfileResponseDto> UpdateProfile(Guid profileId ,UpdateProfileRequestDto request, CancellationToken ct)
        {
            var profile = await context.Profiles
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProfileId == profileId, ct)
                ?? throw new ApiException(
                        "Profile not found.",
                        null,
                        StatusCodes.Status404NotFound);

            var currentUserId = userContext.GetCurrentUserId();
            if (profile.UserId != currentUserId)
                throw new ApiException(
                        "You are not authorized to update this profile.",
                        null,
                        StatusCodes.Status403Forbidden);

            var newImageId = Guid.NewGuid();
            string? savedFilePath = null;
            string? oldFilePath = null;
            var oldImage = profile.Images
                .Where(img => img.ImageOwnerType.ToString() == "Profile")
                .OrderByDescending(img => img.ImageId)
                .FirstOrDefault();

            using var transaction = await context.Database.BeginTransactionAsync(ct);

            try
            {
                if (!string.IsNullOrWhiteSpace(request.DisplayName))
                    profile.DisplayName = request.DisplayName;

                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                    profile.PhoneNumber = request.PhoneNumber;

                if (!string.IsNullOrEmpty(request.ProfilePictureBase64))
                {
                    string dbImageUrl;
                    (dbImageUrl, savedFilePath) = await SaveProfileImage(request.ProfilePictureBase64, newImageId);

                    var newImage = new Images
                    {
                        ImageId = newImageId,
                        ImageUrl = dbImageUrl,
                        OwnerId = profile.ProfileId,
                        ImageOwnerType = Enum.Parse<ImageOwnerType>("Profile"),
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Images.Add(newImage);

                    if (oldImage != null)
                    {
                        oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldImage.ImageUrl.TrimStart('/'));
                    }
                }

                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                if (oldFilePath != null && File.Exists(oldFilePath))
                    File.Delete(oldFilePath);

                return await context.Profiles
                    .AsNoTracking()
                    .Where(p => p.ProfileId == profile.ProfileId)
                    .ProfileToDto()
                    .FirstAsync(ct);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(ct);
                if (savedFilePath != null && File.Exists(savedFilePath))
                    File.Delete(savedFilePath);
                throw;
            }
        }
        private static async Task<(string Url, string PhysicalPath)> SaveProfileImage(string base64, Guid imageId)
        {
            var base64Data = base64.Contains(',') ? base64.Split(',')[1] : base64;
            byte[] imageBytes = Convert.FromBase64String(base64Data);

            using var image = Image.Load(imageBytes);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(400, 400),
                Mode = ResizeMode.Crop
            }));

            var fileName = $"{imageId}.jpg";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fullPath = Path.Combine(folderPath, fileName);
            await image.SaveAsJpegAsync(fullPath);

            return ($"/images/profiles/{fileName}", fullPath);
        }
    }
}
