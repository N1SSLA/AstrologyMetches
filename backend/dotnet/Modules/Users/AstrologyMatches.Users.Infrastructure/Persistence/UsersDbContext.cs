using AstrologyMatches.Users.Application;
using AstrologyMatches.Users.Domain;
using Microsoft.EntityFrameworkCore;

namespace AstrologyMatches.Users.Infrastructure.Persistence;

public class UsersDbContext : DbContext, IUsersDbContext
{
    public const string SchemaName = "users";

    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
