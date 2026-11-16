using AstrologyMatches.Profiles.Application;
using AstrologyMatches.Profiles.Application.Dtos;
using AstrologyMatches.Profiles.Application.Services;
using AstrologyMatches.Profiles.Application.Storage;
using AstrologyMatches.Profiles.Domain;
using AstrologyMatches.Profiles.Infrastructure.Persistence;
using AstrologyMatches.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AstrologyMatches.Profiles.Tests;

public class ProfilePhotoServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Profile _profile;

    // Каждый тест получает свою пустую БД в памяти, поэтому тесты не влияют друг на друга
    private readonly ProfilesDbContext _dbContext;
    private readonly Mock<IPhotoStorage> _photoStorage = new();
    private readonly ProfilePhotoService _service;

    public ProfilePhotoServiceTests()
    {
        var options = new DbContextOptionsBuilder<ProfilesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ProfilesDbContext(options);

        _profile = new Profile(_userId);
        _dbContext.Profiles.Add(_profile);
        _dbContext.SaveChanges();

        _photoStorage
            .Setup(storage => storage.GetDownloadUrlAsync(It.IsAny<string>()))
            .ReturnsAsync((string objectKey) => $"http://storage/{objectKey}");

        var mapperConfiguration = new AutoMapper.MapperConfiguration(
            config => config.AddProfile<ProfilesMappingProfile>(), NullLoggerFactory.Instance);

        _service = new ProfilePhotoService(
            _dbContext,
            _photoStorage.Object,
            mapperConfiguration.CreateMapper(),
            NullLogger<ProfilePhotoService>.Instance);
    }

    [Fact]
    public async Task UploadAsync_FirstJpeg_SavesMainPhotoAndUploadsFile()
    {
        ProfilePhotoDto photo = await _service.UploadAsync(_userId, CreateJpegStream(), CancellationToken.None);

        ProfilePhoto savedPhoto = await _dbContext.ProfilePhotos.SingleAsync();
        Assert.Equal(savedPhoto.Id, photo.Id);
        Assert.True(photo.IsMain);
        Assert.Equal(0, photo.Position);
        Assert.Equal(ImageFormat.Jpeg, savedPhoto.ContentType);
        Assert.Equal($"http://storage/{_profile.Id}/{photo.Id}", photo.Url);
        _photoStorage.Verify(storage => storage.UploadAsync(
            $"{_profile.Id}/{photo.Id}", It.IsAny<Stream>(), ImageFormat.Jpeg, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadAsync_SecondPhoto_IsNotMainAndGoesAfterFirst()
    {
        await _service.UploadAsync(_userId, CreateJpegStream(), CancellationToken.None);

        ProfilePhotoDto second = await _service.UploadAsync(_userId, CreateJpegStream(), CancellationToken.None);

        Assert.False(second.IsMain);
        Assert.Equal(1, second.Position);
    }

    [Fact]
    public async Task UploadAsync_NotAnImage_ThrowsValidationAndDoesNotUpload()
    {
        var textFile = new MemoryStream("just some text file"u8.ToArray());

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UploadAsync(_userId, textFile, CancellationToken.None));

        _photoStorage.Verify(storage => storage.UploadAsync(
            It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UploadAsync_FileTooLarge_ThrowsValidationException()
    {
        var bytes = new byte[ProfilePhoto.MaxFileSizeBytes + 1];
        bytes[0] = 0xFF;
        bytes[1] = 0xD8;
        bytes[2] = 0xFF;

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UploadAsync(_userId, new MemoryStream(bytes), CancellationToken.None));
    }

    [Fact]
    public async Task UploadAsync_PhotoLimitReached_ThrowsValidationException()
    {
        for (int i = 0; i < ProfilePhoto.MaxPhotosPerProfile; i++)
        {
            await _service.UploadAsync(_userId, CreateJpegStream(), CancellationToken.None);
        }

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UploadAsync(_userId, CreateJpegStream(), CancellationToken.None));
    }

    [Fact]
    public async Task UploadAsync_UserWithoutProfile_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UploadAsync(Guid.NewGuid(), CreateJpegStream(), CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_MainPhoto_NextPhotoBecomesMainAndPositionsShift()
    {
        ProfilePhotoDto first = await _service.UploadAsync(_userId, CreateJpegStream(), CancellationToken.None);
        ProfilePhotoDto second = await _service.UploadAsync(_userId, CreateJpegStream(), CancellationToken.None);

        await _service.DeleteAsync(_userId, first.Id, CancellationToken.None);

        ProfilePhoto remaining = await _dbContext.ProfilePhotos.SingleAsync();
        Assert.Equal(second.Id, remaining.Id);
        Assert.True(remaining.IsMain);
        Assert.Equal(0, remaining.Position);
        _photoStorage.Verify(storage => storage.DeleteAsync(
            $"{_profile.Id}/{first.Id}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_PhotoOfAnotherUser_ThrowsNotFoundException()
    {
        ProfilePhotoDto photo = await _service.UploadAsync(_userId, CreateJpegStream(), CancellationToken.None);

        var otherUserId = Guid.NewGuid();
        _dbContext.Profiles.Add(new Profile(otherUserId));
        await _dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DeleteAsync(otherUserId, photo.Id, CancellationToken.None));
    }

    [Fact]
    public async Task SetMainAsync_SecondPhoto_BecomesTheOnlyMain()
    {
        ProfilePhotoDto first = await _service.UploadAsync(_userId, CreateJpegStream(), CancellationToken.None);
        ProfilePhotoDto second = await _service.UploadAsync(_userId, CreateJpegStream(), CancellationToken.None);

        await _service.SetMainAsync(_userId, second.Id, CancellationToken.None);

        IReadOnlyList<ProfilePhotoDto> photos = await _service.GetMyPhotosAsync(_userId, CancellationToken.None);
        Assert.False(photos.Single(p => p.Id == first.Id).IsMain);
        Assert.True(photos.Single(p => p.Id == second.Id).IsMain);
    }

    [Fact]
    public async Task GetPhotosAsync_UnknownProfile_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetPhotosAsync(Guid.NewGuid(), CancellationToken.None));
    }

    private static MemoryStream CreateJpegStream()
    {
        byte[] bytes = { 0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        return new MemoryStream(bytes);
    }
}
