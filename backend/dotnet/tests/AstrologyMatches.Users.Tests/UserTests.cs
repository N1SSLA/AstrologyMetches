using AstrologyMatches.Shared.Exceptions;
using AstrologyMatches.Users.Domain;

namespace AstrologyMatches.Users.Tests;

public class UserTests
{
    [Fact]
    public void Constructor_EmailWithSpacesAndUpperCase_StoresNormalizedEmail()
    {
        var user = new User("  Ivan@Mail.RU ");

        Assert.Equal("ivan@mail.ru", user.Email);
        Assert.Equal(UserStatus.Active, user.Status);
    }

    [Fact]
    public void MarkDeleted_ActiveUser_ErasesEmailAndPassword()
    {
        var user = new User("ivan@mail.ru");
        user.SetPasswordHash("hash");

        user.MarkDeleted();

        Assert.Equal(UserStatus.Deleted, user.Status);
        Assert.DoesNotContain("ivan", user.Email);
        Assert.Equal(string.Empty, user.PasswordHash);
    }

    [Fact]
    public void MarkDeleted_AlreadyDeletedUser_ThrowsValidationException()
    {
        var user = new User("ivan@mail.ru");
        user.MarkDeleted();

        Assert.Throws<ValidationException>(() => user.MarkDeleted());
    }
}
