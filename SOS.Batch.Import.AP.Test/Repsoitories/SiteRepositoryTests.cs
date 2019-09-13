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
    public class SiteRepositoryTests
    {
        private Mock<IDbConnection> _connection;
        private Mock<ISpeciesPortalDataService> _speciesPortalDataServiceMock;
        private Mock<ILogger<SiteRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public SiteRepositoryTests()
        {
            _connection = new Mock<IDbConnection>();
            _speciesPortalDataServiceMock = new Mock<ISpeciesPortalDataService>();
            _loggerMock = new Mock<ILogger<SiteRepository>>();
        }

        [Fact]
        public void ConstructorTest()
        {
            new SiteRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new SiteRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("speciesPortalDataService");

            create = () => new SiteRepository(
                _speciesPortalDataServiceMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        [Fact]
        public async Task GetAsyncSuccess()
        {
            IEnumerable<SiteEntity> projects = new []
            {
                    new SiteEntity { Id = 1, Name = "Site 1" },
                    new SiteEntity { Id = 2, Name = "Site 2" }
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<SiteEntity>(It.IsAny<string>()))
                .ReturnsAsync(projects);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var SiteRepository = new SiteRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await SiteRepository.GetAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<SiteEntity>(It.IsAny<string>()))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var SiteRepository = new SiteRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await SiteRepository.GetAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
    }
}
