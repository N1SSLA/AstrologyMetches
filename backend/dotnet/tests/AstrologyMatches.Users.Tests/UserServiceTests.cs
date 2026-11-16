using AstrologyMatches.Shared.Exceptions;
using AstrologyMatches.Users.Application;
using AstrologyMatches.Users.Application.Authentication;
using AstrologyMatches.Users.Application.Dtos;
using AstrologyMatches.Users.Application.Services;
using AstrologyMatches.Users.Domain;
using AstrologyMatches.Users.Infrastructure.Persistence;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AstrologyMatches.Users.Tests;

public class UserServiceTests
{
    private const string Password = "Secret123!";

    // Каждый тест получает свою пустую БД в памяти, поэтому тесты не влияют друг на друга
    private readonly UsersDbContext _dbContext;
    private readonly UserService _service;

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new UsersDbContext(options);

        var tokenGenerator = new Mock<IJwtTokenGenerator>();
        tokenGenerator
            .Setup(generator => generator.Generate(It.IsAny<User>()))
            .Returns(new JwtToken("test-token", DateTime.UtcNow.AddHours(1)));

        var mapperConfiguration = new MapperConfiguration(
            config => config.AddProfile<UsersMappingProfile>(), NullLoggerFactory.Instance);

        _service = new UserService(
            _dbContext,
            new PasswordHasher<User>(),
            tokenGenerator.Object,
            mapperConfiguration.CreateMapper(),
            NullLogger<UserService>.Instance);
    }

    [Fact]
    public async Task RegisterAsync_NewEmail_SavesUserWithHashedPasswordAndReturnsToken()
    {
        AuthResponse response = await _service.RegisterAsync(new RegisterRequest("Ivan@Mail.ru", Password), CancellationToken.None);

        User savedUser = await _dbContext.Users.SingleAsync();
        Assert.Equal(savedUser.Id, response.UserId);
        Assert.Equal("ivan@mail.ru", savedUser.Email);
        Assert.NotEqual(Password, savedUser.PasswordHash);
        Assert.Equal("test-token", response.AccessToken);
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyTakenInOtherCase_ThrowsConflictException()
    {
        await _service.RegisterAsync(new RegisterRequest("ivan@mail.ru", Password), CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.RegisterAsync(new RegisterRequest(" IVAN@mail.ru", Password), CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_EmailOfDeletedAccount_CreatesNewAccount()
    {
        AuthResponse first = await _service.RegisterAsync(new RegisterRequest("ivan@mail.ru", Password), CancellationToken.None);
        await _service.DeleteAsync(first.UserId, CancellationToken.None);

        AuthResponse second = await _service.RegisterAsync(new RegisterRequest("ivan@mail.ru", Password), CancellationToken.None);

        Assert.NotEqual(first.UserId, second.UserId);
    }

    [Fact]
    public async Task LoginAsync_CorrectPassword_ReturnsToken()
    {
        AuthResponse registered = await _service.RegisterAsync(new RegisterRequest("ivan@mail.ru", Password), CancellationToken.None);

        AuthResponse response = await _service.LoginAsync(new LoginRequest("IVAN@mail.ru", Password), CancellationToken.None);

        Assert.Equal(registered.UserId, response.UserId);
        Assert.Equal("test-token", response.AccessToken);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsValidationException()
    {
        await _service.RegisterAsync(new RegisterRequest("ivan@mail.ru", Password), CancellationToken.None);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.LoginAsync(new LoginRequest("ivan@mail.ru", "WrongPassword"), CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.LoginAsync(new LoginRequest("nobody@mail.ru", Password), CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_DeletedAccount_ThrowsValidationException()
    {
        AuthResponse registered = await _service.RegisterAsync(new RegisterRequest("ivan@mail.ru", Password), CancellationToken.None);
        await _service.DeleteAsync(registered.UserId, CancellationToken.None);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.LoginAsync(new LoginRequest("ivan@mail.ru", Password), CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_ActiveUser_ReturnsUserDto()
    {
        AuthResponse registered = await _service.RegisterAsync(new RegisterRequest("ivan@mail.ru", Password), CancellationToken.None);

        UserDto user = await _service.GetByIdAsync(registered.UserId, CancellationToken.None);

        Assert.Equal(registered.UserId, user.Id);
        Assert.Equal("ivan@mail.ru", user.Email);
    }

    [Fact]
    public async Task DeleteAsync_ActiveUser_MarksAccountDeleted()
    {
        AuthResponse registered = await _service.RegisterAsync(new RegisterRequest("ivan@mail.ru", Password), CancellationToken.None);

        await _service.DeleteAsync(registered.UserId, CancellationToken.None);

        User savedUser = await _dbContext.Users.SingleAsync();
        Assert.Equal(UserStatus.Deleted, savedUser.Status);
    }

    [Fact]
    public async Task DeleteAsync_UnknownUser_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DeleteAsync(Guid.NewGuid(), CancellationToken.None));
    }
}
