using AstrologyMatches.Users.Application;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace AstrologyMatches.Users.Tests;

public class UsersMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = new MapperConfiguration(
            config => config.AddProfile<UsersMappingProfile>(), NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }
}
