using AstrologyMatches.Profiles.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AstrologyMatches.Profiles.Infrastructure.Persistence;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("profiles");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        // У одного аккаунта одна анкета. FK на users.users не делаем: между модулями храним только Id
        builder.HasIndex(p => p.UserId).IsUnique();
    }
}
