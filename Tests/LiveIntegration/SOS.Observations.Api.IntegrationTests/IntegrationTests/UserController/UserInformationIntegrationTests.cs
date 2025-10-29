using FluentAssertions;
using SOS.Lib.Models.UserService;
using SOS.Observations.Api.LiveIntegrationTests.Extensions;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.UserController
{
    [Collection(Fixtures.Collections.ApiIntegrationTestsCollection)]
    public class UserInformationIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public UserInformationIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task GetUserInformation()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _fixture.UseUserServiceWithToken(_fixture.UserAuthenticationToken);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.UserController.GetUserInformation();
            var userInformation = response.GetResult<UserInformation>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            userInformation.Should().NotBeNull();
        }
    }
}