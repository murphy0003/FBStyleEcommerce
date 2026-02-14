using InternProject.Dtos;

namespace InternProject.Services.BlueMarkService
{
    public interface IBlueMarkService
    {
        Task<BlueMarkResponseDto> CreateBlueMarkAsync(BlueMarkCreateDto blueMarkCreateDto , CancellationToken cancellationToken);
        Task<BlueMarkResponseDto> UpdateBlueMarkAsync(BlueMarkUpdateDto redMarkUpdateDto , CancellationToken cancellationToken);
    }
}
