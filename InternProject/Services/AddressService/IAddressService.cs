using InternProject.Dtos;

namespace InternProject.Services.AddressService
{
    public interface IAddressService
    {
            Task CreateAddressAsync(AddressCreateDto addressCreateDto, CancellationToken cancellationToken);
            Task UpdateAddressAsync(Guid addressId, AddressUpdateDto addressUpdateDto, CancellationToken cancellationToken);
            Task DeleteAddressAsync(Guid addressId, CancellationToken cancellationToken);
            Task<AddressDto> GetAddressAsync(Guid addressId, CancellationToken cancellationToken);
            Task<AddressDto> GetDefaultAddressAsync(CancellationToken cancellationToken);
            Task<List<AddressDto>> GetUserAddressesAsync(CancellationToken cancellationToken);
    }
}
