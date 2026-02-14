using InternProject.Dtos;
using InternProject.Models.UserModels;
namespace InternProject.Extensions
{
    public static class UserMappings
    {
        public static User ToV1Model(RegisterV1UserDto registerUserDto)
        {
            return new User
            {
                UserName = registerUserDto.UserName,
                Email = registerUserDto.Email,
                Password = registerUserDto.Password,
                PhoneNumber = registerUserDto.PhoneNumber,
                Type = Enum.TryParse<AccountType>(registerUserDto.AccountType, true, out var type)
                   ? type
                   : AccountType.Buyer,
                CreatedAt = DateTime.Now,
                Status = AccountStatus.Pending
            };
        }
        public static User ToV2Model(RegisterV2UserInitDto registerV2UserInitDto)
        {
            return new User
            {
                UserName = registerV2UserInitDto.UserName,
                Email = registerV2UserInitDto.Email,
                Type = Enum.TryParse<AccountType>(registerV2UserInitDto.AccountType, true, out var type)
                   ? type
                   : AccountType.Seller,
                CreatedAt = DateTime.Now,
                Status = AccountStatus.Pending
            };
        }

    }
}
