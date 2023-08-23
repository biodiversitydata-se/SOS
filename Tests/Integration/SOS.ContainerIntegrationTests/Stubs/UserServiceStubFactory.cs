using NSubstitute;
using SOS.Lib.Models.UserService;
using SOS.Lib.Services.Interfaces;
using SOS.TestHelpers.Helpers.Builders;

namespace SOS.ContainerIntegrationTests.Stubs;
internal static class UserServiceStubFactory
{
    public static IUserService CreateWithSightingAuthority(int maxProtectionLevel)
    {
        return CreateWithSightingAuthority(15, maxProtectionLevel);
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
}
