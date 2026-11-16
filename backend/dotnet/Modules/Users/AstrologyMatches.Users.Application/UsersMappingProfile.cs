using AstrologyMatches.Users.Application.Dtos;
using AstrologyMatches.Users.Domain;
using AutoMapper;

namespace AstrologyMatches.Users.Application;

public class UsersMappingProfile : Profile
{
    public UsersMappingProfile()
    {
        CreateMap<User, UserDto>();
    }
}
