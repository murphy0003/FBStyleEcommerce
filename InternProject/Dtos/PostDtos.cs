using InternProject.Models;
using InternProject.Models.ImageModels;
using System.ComponentModel.DataAnnotations;

namespace InternProject.Dtos
{
    public record PostCreateDto([Required] string ItemName , string Description ,[Range(0.01,1000000000)] decimal Price , string ItemStatus , string ItemCondition , List<string> PostImages);
    public record PostResponseDto(Guid PostId , string ItemName , string Description , decimal Price , string ItemStatus , string ItemCondition , List<ImageResultDto> Images);
    public record ImageResultDto(Guid ImageId , ImageStatus Status);
    public record PostUpdateDto(string ItemName , string Description , [Range(0.01, 1000000000)] decimal Price , string ItemStatus , string ItemCondition , List<Guid> KeepImages , List<string> NewImages);
    public record GetPostResponseDto(Guid PostId , string ItemName , string Description , decimal Price , string ItemStatus , string ItemCondition , Guid SellerId , DateTime CreatedAt , DateTime UpdatedAt , List<PostImageDto> Images);
    public record PostImageDto(Guid ImageId , string Url , string Status);
}
