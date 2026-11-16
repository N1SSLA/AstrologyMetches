using AstrologyMatches.Profiles.Application.Dtos;
using AstrologyMatches.Profiles.Domain;

namespace AstrologyMatches.Profiles.Application;

// Полное имя AutoMapper.Profile: в Domain тоже есть класс Profile (анкета пользователя)
public class ProfilesMappingProfile : AutoMapper.Profile
{
    public ProfilesMappingProfile()
    {
        // Url выдаёт хранилище изображений, его заполняет ProfilePhotoService
        CreateMap<ProfilePhoto, ProfilePhotoDto>()
            .ForMember(dto => dto.Url, options => options.Ignore());
    }
}
