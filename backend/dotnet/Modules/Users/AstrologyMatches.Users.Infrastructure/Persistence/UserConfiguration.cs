using AstrologyMatches.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AstrologyMatches.Users.Infrastructure.Persistence;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).IsRequired();

        // Храним статус строкой ('Active' / 'Deleted'), чтобы в БД было понятно без расшифровки чисел
        builder.Property(u => u.Status).HasConversion<string>().HasMaxLength(20);
    }
}
