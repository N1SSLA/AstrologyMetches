using AstrologyMatches.Shared.Exceptions;
using AstrologyMatches.Users.Application.Authentication;
using AstrologyMatches.Users.Application.Dtos;
using AstrologyMatches.Users.Domain;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AstrologyMatches.Users.Application.Services;

public class UserService : IUserService
{
    // Одинаковое сообщение для «нет такого email» и «неверный пароль»: так по ответу нельзя узнать, зарегистрирован ли адрес
    private const string InvalidCredentialsMessage = "Неверный email или пароль.";

    private readonly IUsersDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUsersDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        string email = User.NormalizeEmail(request.Email);

        bool emailTaken = await _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (emailTaken)
        {
            throw new ConflictException("Пользователь с таким email уже зарегистрирован.");
        }

        var user = new User(email);
        user.SetPasswordHash(_passwordHasher.HashPassword(user, request.Password));

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Зарегистрирован пользователь {UserId}", user.Id);

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        string email = User.NormalizeEmail(request.Email);

        User? user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.Status == UserStatus.Active, cancellationToken);
        if (user == null)
        {
            throw new ValidationException(InvalidCredentialsMessage);
        }

        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new ValidationException(InvalidCredentialsMessage);
        }

        // Хеш сделан устаревшим алгоритмом: пароль верный, поэтому пересохраняем его по-новому
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.SetPasswordHash(_passwordHasher.HashPassword(user, request.Password));
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return CreateAuthResponse(user);
    }

    public async Task<UserDto> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        User user = await GetActiveUserAsync(userId, cancellationToken);
        return _mapper.Map<UserDto>(user);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        User user = await GetActiveUserAsync(userId, cancellationToken);

        user.MarkDeleted();
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Удалён аккаунт пользователя {UserId}", user.Id);
    }

    private async Task<User> GetActiveUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        User? user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.Status == UserStatus.Active, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Пользователь не найден.");
        }

        return user;
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        JwtToken token = _tokenGenerator.Generate(user);
        return new AuthResponse(user.Id, token.Value, token.ExpiresAt);
    }
}
