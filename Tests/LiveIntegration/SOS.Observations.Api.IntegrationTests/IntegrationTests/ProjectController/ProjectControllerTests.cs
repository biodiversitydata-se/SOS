using FluentAssertions;
using SOS.Observations.Api.LiveIntegrationTests.Extensions;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using SOS.Shared.Api.Dtos.Vocabulary;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.ProjectsController
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class ProjectControllerTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public ProjectControllerTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task GetProjects()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            var response = await _fixture.ProjectsController.GetProjectes(null);
            var result = response.GetResult<IEnumerable<ProjectDto>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Count().Should().BeGreaterThan(0);
        }
    }
}
