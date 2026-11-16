using AstrologyMatches.Profiles.Application;
using AstrologyMatches.Profiles.Domain;
using Microsoft.EntityFrameworkCore;

namespace AstrologyMatches.Profiles.Infrastructure.Persistence;

public class ProfilesDbContext : DbContext, IProfilesDbContext
{
    public const string SchemaName = "profiles";

    public ProfilesDbContext(DbContextOptions<ProfilesDbContext> options) : base(options)
    {
    }

    public DbSet<Profile> Profiles => Set<Profile>();

    public DbSet<ProfilePhoto> ProfilePhotos => Set<ProfilePhoto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfiguration(new ProfileConfiguration());
        modelBuilder.ApplyConfiguration(new ProfilePhotoConfiguration());
    }
}
