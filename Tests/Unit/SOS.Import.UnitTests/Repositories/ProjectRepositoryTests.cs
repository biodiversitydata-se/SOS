using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Entities;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Services.Interfaces;
using Xunit;

namespace SOS.Import.UnitTests.Repositories
{
    /// <summary>
    /// Project tests
    /// </summary>
    public class ProjectRepositoryTests
    {
        private readonly Mock<ISpeciesPortalDataService> _speciesPortalDataServiceMock;
        private readonly Mock<ILogger<ProjectRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectRepositoryTests()
        {
            _speciesPortalDataServiceMock = new Mock<ISpeciesPortalDataService>();
            _loggerMock = new Mock<ILogger<ProjectRepository>>();
        }

        /// <summary>
        /// Test the constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new ProjectRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new ProjectRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("speciesPortalDataService");

            create = () => new ProjectRepository(
                _speciesPortalDataServiceMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }


        /// <summary>
        /// Test get projects success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsyncSuccess()
        {
            IEnumerable<ProjectEntity> projects = new []
            {
                    new ProjectEntity { Id = 1, Name = "Project 1" },
                    new ProjectEntity { Id = 2, Name = "Project 2" }
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<ProjectEntity>(It.IsAny<string>(), null))
                .ReturnsAsync(projects);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var ProjectRepository = new ProjectRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await ProjectRepository.GetProjectsAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Test get projects fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<ProjectEntity>(It.IsAny<string>(), null))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var ProjectRepository = new ProjectRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await ProjectRepository.GetProjectsAsync();
            
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }


    }
}
