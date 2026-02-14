using InternProject.Data;
using InternProject.Dtos;
using InternProject.Extensions;
using InternProject.Models.ApiModels;
using InternProject.Services.UserService;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;

namespace InternProject.Services.AddressService
{
    public class AddressService(AppDbContext context, IUserContext userContext) : IAddressService
    {
        public async Task CreateAddressAsync(AddressCreateDto addressCreateDto, CancellationToken cancellationToken)
        {
            var userId = userContext.GetCurrentUserId();
            var address = AddressMappings.ToModel(addressCreateDto);
            address.UserId = userId;
            bool hasDefault = await context.Addresses.AnyAsync(a => a.UserId == userId && a.IsDefault, cancellationToken);
            if (!hasDefault)
            {
                address.IsDefault = true;
            }
            if(address.IsDefault)
            {
                await SetOthersNotDefaultAsync(userId, null, cancellationToken);
            }
            await context.Addresses.AddAsync(address, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        public async Task UpdateAddressAsync(Guid addressId, AddressUpdateDto addressUpdateDto, CancellationToken cancellationToken)
        {
            var userId = userContext.GetCurrentUserId();
            var address = await context.Addresses.FindAsync([addressId], cancellationToken);
            if (address == null || address.UserId != userId)
            {
                throw new ApiException("ADDRESS_NOT_FOUND", null, StatusCodes.Status404NotFound);
            }
            if(addressUpdateDto.IsDefault && !address.IsDefault)
            {
                await SetOthersNotDefaultAsync(userId, addressId, cancellationToken);
            }
            else if(!addressUpdateDto.IsDefault && address.IsDefault)
            {
                throw new ApiException("AT_LEAST_ONE_DEFAULT_ADDRESS_REQUIRED", null, StatusCodes.Status400BadRequest);
            }
            AddressMappings.UpdateModel(address, addressUpdateDto);
            context.Addresses.Update(address);
            await context.SaveChangesAsync(cancellationToken);
        }
        public async Task DeleteAddressAsync(Guid addressId, CancellationToken cancellationToken)
        {
            var userId = userContext.GetCurrentUserId();
            var address = await context.Addresses.FindAsync([addressId], cancellationToken);
            if (address == null || address.UserId != userId)
            {
                throw new ApiException("ADDRESS_NOT_FOUND", null, StatusCodes.Status404NotFound);
            }
            bool wasDefault = address.IsDefault;
            context.Addresses.Remove(address);
            await context.SaveChangesAsync(cancellationToken);
            if(wasDefault)
            {
                var latestAddress = await context.Addresses.Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);
                if (latestAddress != null)
                {
                    latestAddress.IsDefault = true;
                    await context.SaveChangesAsync(cancellationToken);
                }
            }
        }
        public async Task<AddressDto> GetAddressAsync(Guid addressId, CancellationToken cancellationToken)
        {
            var address = await context.Addresses.FindAsync([addressId], cancellationToken);
            if (address == null || address.UserId != userContext.GetCurrentUserId())
            {
                throw new ApiException("ADDRESS_NOT_FOUND", null, StatusCodes.Status404NotFound);
            }
            return AddressMappings.ToDto(address);
        }
        public async Task <AddressDto> GetDefaultAddressAsync(CancellationToken cancellationToken)
        {
            var userId = userContext.GetCurrentUserId();
            var address = await context.Addresses.FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault, cancellationToken);
            return address == null
                ? throw new ApiException("DEFAULT_ADDRESS_NOT_FOUND", null, StatusCodes.Status404NotFound)
                : AddressMappings.ToDto(address);
        }
        public async Task<List<AddressDto>> GetUserAddressesAsync(CancellationToken cancellationToken)
        {
            var userId = userContext.GetCurrentUserId();
            var addresses = await context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync(cancellationToken);
            return [.. addresses.Select(AddressMappings.ToDto)];
        }
        private async Task SetOthersNotDefaultAsync(Guid userId , Guid? excludeId , CancellationToken cancellationToken = default)
        {
            await context.Addresses.Where(a=> a.UserId == userId && a.AddressId != excludeId && a.IsDefault)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false), cancellationToken);
        }
    }
}
