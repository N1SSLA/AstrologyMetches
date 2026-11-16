using AstrologyMatches.Profiles.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AstrologyMatches.Profiles.Infrastructure.Persistence;

public class ProfilePhotoConfiguration : IEntityTypeConfiguration<ProfilePhoto>
{
    public void Configure(EntityTypeBuilder<ProfilePhoto> builder)
    {
        builder.ToTable("profile_photos");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.ContentType).HasMaxLength(50).IsRequired();

        // ObjectKey вычисляется из ProfileId и Id, отдельно его не храним
        builder.Ignore(p => p.ObjectKey);

        // Таблицы одного модуля, поэтому FK можно. При удалении анкеты удаляются и записи о её фото
        builder.HasOne<Profile>()
            .WithMany()
            .HasForeignKey(p => p.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
