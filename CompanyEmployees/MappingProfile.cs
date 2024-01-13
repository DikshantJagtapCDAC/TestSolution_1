using AutoMapper;
using CompanyEmployees.Entities.DataTransferObjects;
using CompanyEmployees.Entities.Models;

namespace CompanyEmployees
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserForRegistrationDto, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            CreateMap<User, UserForRegistrationDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.UserName));

            CreateMap<User, UserResponseDto>();

            CreateMap<UserForUpdate, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Exclude Id property from mapping
            // Map other properties here
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); 



        }
    }
}
