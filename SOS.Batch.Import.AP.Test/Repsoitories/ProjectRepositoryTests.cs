using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Batch.Import.AP.Entities;
using SOS.Batch.Import.AP.Repositories.Source;
using SOS.Batch.Import.AP.Services.Interfaces;
using Xunit;

namespace SOS.Batch.Import.AP.Test
{
    public class ProjectRepositoryTests
    {
        private Mock<IDbConnection> _connection;
        private Mock<ISpeciesPortalDataService> _speciesPortalDataServiceMock;
        private Mock<ILogger<ProjectRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectRepositoryTests()
        {
            _connection = new Mock<IDbConnection>();
            _speciesPortalDataServiceMock = new Mock<ISpeciesPortalDataService>();
            _loggerMock = new Mock<ILogger<ProjectRepository>>();
        }

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

        [Fact]
        public async Task GetAsyncSuccess()
        {
            IEnumerable<ProjectEntity> projects = new []
            {
                    new ProjectEntity { Id = 1, Name = "Project 1" },
                    new ProjectEntity { Id = 2, Name = "Project 2" }
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<ProjectEntity>(It.IsAny<string>()))
                .ReturnsAsync(projects);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var ProjectRepository = new ProjectRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await ProjectRepository.GetAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<ProjectEntity>(It.IsAny<string>()))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var ProjectRepository = new ProjectRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await ProjectRepository.GetAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
    }
}
