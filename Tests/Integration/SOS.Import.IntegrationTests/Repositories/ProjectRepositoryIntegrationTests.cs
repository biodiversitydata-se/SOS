using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Entities;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Services;
using Xunit;

namespace SOS.Import.IntegrationTests.Repositories
{
    public class ProjectRepositoryIntegrationTests : TestBase
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestGetAllProjects()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var importConfiguration = GetImportConfiguration();
            var speciesPortalDataService = new SpeciesPortalDataService(importConfiguration.SpeciesPortalConfiguration);

            ProjectRepository projectRepository = new ProjectRepository(
                speciesPortalDataService,
                new Mock<ILogger<ProjectRepository>>().Object);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            IEnumerable<ProjectEntity> projectEntities = await projectRepository.GetProjectsAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            projectEntities.Should().NotBeEmpty();
        }
    }
}