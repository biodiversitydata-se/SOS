using NSubstitute;
using SOS.Lib.Models.UserService;
using SOS.Lib.Services.Interfaces;
using SOS.TestHelpers.Helpers.Builders;

namespace SOS.ContainerIntegrationTests.Stubs;
internal static class UserServiceStubFactory
{
    public static IUserService CreateWithSightingAuthority(int maxProtectionLevel)
    {
        return CreateWithSightingAuthority(TestAuthHandler.DefaultTestUserId, maxProtectionLevel);
    }

    public static IUserService CreateWithSightingAuthority(int userId, int maxProtectionLevel)
    {
        var authorityBuilder = new UserAuthorizationTestBuilder();
        var authority = authorityBuilder
            .WithAuthorityIdentity("Sighting")
            .WithMaxProtectionLevel(maxProtectionLevel)
            .Build();
        var authorities = new List<AuthorityModel> { authority };

        UserModel user = new UserModel();
        user.Id = userId;
        IUserService userServiceMock = Substitute.For<IUserService>();
        userServiceMock.GetUserAsync().Returns(user);
        userServiceMock.GetUserAuthoritiesAsync(userId, Arg.Any<string>(), Arg.Any<string>()).Returns(authorities);
        
        return userServiceMock;
    }

    public static IUserService CreateWithCountySightingIndicationAuthority(int maxProtectionLevel, string countyFeatureId)
    {
        return CreateWithCountySightingIndicationAuthority(TestAuthHandler.DefaultTestUserId, maxProtectionLevel, countyFeatureId);
    }

    public static IUserService CreateWithCountySightingIndicationAuthority(int userId, int maxProtectionLevel, string countyFeatureId)
    {
        var authorityBuilder = new UserAuthorizationTestBuilder();
        var authority = authorityBuilder
            .WithAuthorityIdentity("SightingIndication")
            .WithMaxProtectionLevel(maxProtectionLevel)
            .WithAreaAccess(Lib.Enums.AreaType.County, countyFeatureId)
            .Build();
        var authorities = new List<AuthorityModel> { authority };

        UserModel user = new UserModel();
        user.Id = userId;
        IUserService userServiceMock = Substitute.For<IUserService>();
        userServiceMock.GetUserAsync().Returns(user);
        userServiceMock.GetUserAuthoritiesAsync(userId, Arg.Any<string>(), Arg.Any<string>()).Returns(authorities);

        return userServiceMock;
    }
}
