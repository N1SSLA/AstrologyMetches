using AstrologyMatches.Profiles.Application;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace AstrologyMatches.Profiles.Tests;

public class ProfilesMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = new MapperConfiguration(
            config => config.AddProfile<ProfilesMappingProfile>(), NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }
}
