using InternProject.Dtos;
using InternProject.Services.AddressService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InternProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController(IAddressService addressService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> CreateAddress(AddressCreateDto addressCreateDto, CancellationToken cancellationToken)
        {
            await addressService.CreateAddressAsync(addressCreateDto, cancellationToken);
            HttpContext.Items["ResponseMessage"] = "Address created successfully.";
            Response.Headers.CacheControl = "no-cache";
            return Accepted();
        }
        [HttpPatch("{addressId:guid}")]
        public async Task<ActionResult> UpdateAddress(Guid addressId, AddressUpdateDto addressUpdateDto, CancellationToken cancellationToken)
        {
            await addressService.UpdateAddressAsync(addressId, addressUpdateDto, cancellationToken);
            HttpContext.Items["ResponseMessage"] = "Address updated successfully.";
            Response.Headers.CacheControl = "no-cache";
            return Accepted();
        }
        [HttpDelete("{addressId:guid}")]
        public async Task<ActionResult> DeleteAddress(Guid addressId, CancellationToken cancellationToken)
        {
            await addressService.DeleteAddressAsync(addressId, cancellationToken);
            HttpContext.Items["ResponseMessage"] = "Address deleted successfully.";
            return NoContent();
        }
        [HttpGet("{addressId:guid}")]
        public async Task<ActionResult> GetAddress(Guid addressId, CancellationToken cancellationToken)
        {
            var result = await addressService.GetAddressAsync(addressId, cancellationToken);
            HttpContext.Items["ResponseMessage"] = "Address retrieved successfully.";
            Response.Headers.CacheControl = "no-cache";
            return Ok(result);
        }
        [HttpGet]
        public async Task<ActionResult> GetDefaultAddress(CancellationToken cancellationToken)
        {
            var result = await addressService.GetDefaultAddressAsync(cancellationToken);
            HttpContext.Items["ResponseMessage"] = "Default address retrieved successfully.";
            Response.Headers.CacheControl = "no-cache";
            return Ok(result);
        }
        [HttpGet("List")]
        public async Task<ActionResult> GetUserAddresses(CancellationToken cancellationToken)
        {
            var result = await addressService.GetUserAddressesAsync(cancellationToken);
            HttpContext.Items["ResponseMessage"] = "User addresses retrieved successfully.";
            Response.Headers.CacheControl = "no-cache";
            return Ok(result);
        }
    }
}
