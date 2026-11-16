using AstrologyMatches.Users.Domain;
using Microsoft.EntityFrameworkCore;

namespace AstrologyMatches.Users.Application;

/// <summary>
/// Доступ к таблицам модуля Users. Реализуется в Infrastructure (UsersDbContext).
/// </summary>
public interface IUsersDbContext
{
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
