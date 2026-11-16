using AstrologyMatches.Profiles.Application.Dtos;
using AstrologyMatches.Profiles.Application.Storage;
using AstrologyMatches.Profiles.Domain;
using AstrologyMatches.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AstrologyMatches.Profiles.Application.Services;

public class ProfilePhotoService : IProfilePhotoService
{
    private readonly IProfilesDbContext _dbContext;
    private readonly IPhotoStorage _photoStorage;
    // Полное имя: в Domain есть свой класс Profile, и using AutoMapper сделал бы имя Profile неоднозначным
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<ProfilePhotoService> _logger;

    public ProfilePhotoService(
        IProfilesDbContext dbContext,
        IPhotoStorage photoStorage,
        AutoMapper.IMapper mapper,
        ILogger<ProfilePhotoService> logger)
    {
        _dbContext = dbContext;
        _photoStorage = photoStorage;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProfilePhotoDto>> GetMyPhotosAsync(Guid userId, CancellationToken cancellationToken)
    {
        Profile profile = await GetProfileByUserIdAsync(userId, cancellationToken);
        return await GetPhotoDtosAsync(profile.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<ProfilePhotoDto>> GetPhotosAsync(Guid profileId, CancellationToken cancellationToken)
    {
        bool profileExists = await _dbContext.Profiles.AnyAsync(p => p.Id == profileId, cancellationToken);
        if (!profileExists)
        {
            throw new NotFoundException("Профиль не найден.");
        }

        return await GetPhotoDtosAsync(profileId, cancellationToken);
    }

    public async Task<ProfilePhotoDto> UploadAsync(Guid userId, Stream content, CancellationToken cancellationToken)
    {
        if (content.Length == 0)
        {
            throw new ValidationException("Файл пустой.");
        }

        if (content.Length > ProfilePhoto.MaxFileSizeBytes)
        {
            throw new ValidationException($"Размер фото не должен превышать {ProfilePhoto.MaxFileSizeBytes / 1024 / 1024} МБ.");
        }

        Profile profile = await GetProfileByUserIdAsync(userId, cancellationToken);

        int photoCount = await _dbContext.ProfilePhotos.CountAsync(p => p.ProfileId == profile.Id, cancellationToken);
        if (photoCount >= ProfilePhoto.MaxPhotosPerProfile)
        {
            throw new ValidationException($"Можно загрузить не больше {ProfilePhoto.MaxPhotosPerProfile} фото.");
        }

        string? contentType = await DetectContentTypeAsync(content, cancellationToken);
        if (contentType == null)
        {
            throw new ValidationException("Можно загружать только изображения JPEG, PNG или WEBP.");
        }

        // Первое фото сразу становится главным
        var photo = new ProfilePhoto(profile.Id, contentType, position: photoCount, isMain: photoCount == 0);

        await _photoStorage.UploadAsync(photo.ObjectKey, content, contentType, cancellationToken);

        _dbContext.ProfilePhotos.Add(photo);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Метаданные не сохранились: убираем файл, чтобы он не остался в хранилище без записи в БД
            await _photoStorage.DeleteAsync(photo.ObjectKey, CancellationToken.None);
            throw;
        }

        _logger.LogInformation("Загружено фото {PhotoId} для профиля {ProfileId}", photo.Id, profile.Id);

        return await ToDtoAsync(photo);
    }

    public async Task DeleteAsync(Guid userId, Guid photoId, CancellationToken cancellationToken)
    {
        Profile profile = await GetProfileByUserIdAsync(userId, cancellationToken);

        List<ProfilePhoto> photos = await GetPhotosOrderedAsync(profile.Id, cancellationToken);
        ProfilePhoto? photo = photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null)
        {
            throw new NotFoundException("Фото не найдено.");
        }

        _dbContext.ProfilePhotos.Remove(photo);
        photos.Remove(photo);

        // Оставшиеся фото сдвигаем, чтобы позиции снова шли 0, 1, 2…
        for (int position = 0; position < photos.Count; position++)
        {
            photos[position].MoveTo(position);
        }

        if (photo.IsMain && photos.Count > 0)
        {
            photos[0].MakeMain();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Файл удаляем после БД: если это не получится, останется лишний файл, но не битая ссылка в профиле
        try
        {
            await _photoStorage.DeleteAsync(photo.ObjectKey, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Не удалось удалить файл {ObjectKey} из хранилища", photo.ObjectKey);
        }
    }

    public async Task SetMainAsync(Guid userId, Guid photoId, CancellationToken cancellationToken)
    {
        Profile profile = await GetProfileByUserIdAsync(userId, cancellationToken);

        List<ProfilePhoto> photos = await GetPhotosOrderedAsync(profile.Id, cancellationToken);
        if (!photos.Any(p => p.Id == photoId))
        {
            throw new NotFoundException("Фото не найдено.");
        }

        foreach (ProfilePhoto photo in photos)
        {
            if (photo.Id == photoId)
            {
                photo.MakeMain();
            }
            else
            {
                photo.RemoveMain();
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Profile> GetProfileByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        Profile? profile = await _dbContext.Profiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (profile == null)
        {
            throw new NotFoundException("Профиль не найден. Сначала заполните профиль.");
        }

        return profile;
    }

    private async Task<List<ProfilePhoto>> GetPhotosOrderedAsync(Guid profileId, CancellationToken cancellationToken)
    {
        return await _dbContext.ProfilePhotos
            .Where(p => p.ProfileId == profileId)
            .OrderBy(p => p.Position)
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<ProfilePhotoDto>> GetPhotoDtosAsync(Guid profileId, CancellationToken cancellationToken)
    {
        List<ProfilePhoto> photos = await GetPhotosOrderedAsync(profileId, cancellationToken);

        var result = new List<ProfilePhotoDto>();
        foreach (ProfilePhoto photo in photos)
        {
            result.Add(await ToDtoAsync(photo));
        }

        return result;
    }

    private async Task<ProfilePhotoDto> ToDtoAsync(ProfilePhoto photo)
    {
        ProfilePhotoDto dto = _mapper.Map<ProfilePhotoDto>(photo);
        string url = await _photoStorage.GetDownloadUrlAsync(photo.ObjectKey);
        return dto with { Url = url };
    }

    private static async Task<string?> DetectContentTypeAsync(Stream content, CancellationToken cancellationToken)
    {
        var header = new byte[ImageFormat.HeaderLength];
        int bytesRead = await content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken);

        // Возвращаемся в начало, чтобы потом загрузить файл целиком
        content.Position = 0;

        return ImageFormat.DetectContentType(header.Take(bytesRead).ToArray());
    }
}
