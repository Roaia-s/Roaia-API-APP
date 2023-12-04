using AutoMapper;

namespace Roaia.Core.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        //Users
        CreateMap<ApplicationUser, UserDto>();

        CreateMap<UserFormDto, ApplicationUser>()
            .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
            .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.UserName.ToUpper()))
            .ReverseMap();
    }
}
