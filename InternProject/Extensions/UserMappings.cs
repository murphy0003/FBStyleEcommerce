using InternProject.Dtos;
using InternProject.Models.UserModels;
namespace InternProject.Extensions
{
    public static class UserMappings
    {
        public static Users ToV1Model(RegisterV1UserDto registerUserDto)
        {
            return new Users
            {
                UserName = registerUserDto.UserName,
                Email = registerUserDto.Email,
                Password = registerUserDto.Password,
                Phone = registerUserDto.Phone,
                Type = Enum.TryParse<AccountType>(registerUserDto.AccountType, true, out var type)
                   ? type
                   : AccountType.Buyer,
                CreatedAt = DateTime.Now,
                Status = AccountStatus.Pending
            };
        }
        public static Users ToV2Model(RegisterV2UserInitDto registerV2UserInitDto)
        {
            return new Users
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
