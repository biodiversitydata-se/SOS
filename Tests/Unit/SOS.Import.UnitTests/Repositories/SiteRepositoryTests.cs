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

namespace SOS.Import.Test.Repositories
{
    /// <summary>
    /// Test site repository
    /// </summary>
    public class SiteRepositoryTests
    {
        private readonly Mock<ISpeciesPortalDataService> _speciesPortalDataServiceMock;
        private readonly Mock<ILogger<SiteRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public SiteRepositoryTests()
        {
            _speciesPortalDataServiceMock = new Mock<ISpeciesPortalDataService>();
            _loggerMock = new Mock<ILogger<SiteRepository>>();
        }

        /// <summary>
        /// Constructor tests
        /// </summary>
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

        /// <summary>
        /// Test get sites success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsyncSuccess()
        {
            IEnumerable<SiteEntity> projects = new []
            {
                    new SiteEntity { Id = 1, Name = "Site 1" },
                    new SiteEntity { Id = 2, Name = "Site 2" }
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<SiteEntity>(It.IsAny<string>(), null))
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

        /// <summary>
        /// Test get sites fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<SiteEntity>(It.IsAny<string>(), null))
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
