using InternProject.Dtos;
using InternProject.Models.AddressModels;
using InternProject.Models.UserModels;

namespace InternProject.Extensions
{
    public static class AddressMappings
    {
        public static Address ToModel(AddressCreateDto addressCreateDto)
        {
            return new Address
            {
                AddressName = addressCreateDto.Address,
                DeliveryInstructions = addressCreateDto.DeliveryInstructions,
                CreatedAt = DateTime.UtcNow
            };
        }
        public static AddressDto ToDto(Address address)
        {
            return new AddressDto
            (
                address.AddressId,
                address.AddressName,
                address.DeliveryInstructions,
                address.IsDefault
            );
        }
        public static Address UpdateModel(Address address, AddressUpdateDto addressUpdateDto)
        {
            if (!string.IsNullOrWhiteSpace(addressUpdateDto.Address))
                address.AddressName = addressUpdateDto.Address.Trim();
            if (addressUpdateDto.DeliveryInstructions is not null)
                address.DeliveryInstructions = addressUpdateDto.DeliveryInstructions.Trim();
            address.IsDefault = addressUpdateDto.IsDefault;
            address.UpdatedAt = DateTime.UtcNow;
            return address;
        }


    }
}
