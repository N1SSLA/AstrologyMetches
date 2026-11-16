using AstrologyMatches.Profiles.Domain;
using Microsoft.EntityFrameworkCore;

namespace AstrologyMatches.Profiles.Application;

/// <summary>
/// Доступ к таблицам модуля Profiles. Реализуется в Infrastructure (ProfilesDbContext).
/// </summary>
public interface IProfilesDbContext
{
    DbSet<Profile> Profiles { get; }

    DbSet<ProfilePhoto> ProfilePhotos { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
