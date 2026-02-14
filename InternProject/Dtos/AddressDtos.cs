using System.ComponentModel.DataAnnotations;

namespace InternProject.Dtos
{
    public record AddressCreateDto([Required] string Address , string DeliveryInstructions);
    public record AddressUpdateDto(string Address, string DeliveryInstructions, bool IsDefault);
    public record AddressDto(Guid AddressId, string Address, string DeliveryInstructions, bool IsDefault);

}
