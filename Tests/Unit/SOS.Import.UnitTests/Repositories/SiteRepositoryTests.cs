using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Entities;
using SOS.Import.Repositories.Source.Artportalen;
using SOS.Import.Services.Interfaces;
using Xunit;

namespace SOS.Import.UnitTests.Repositories
{
    /// <summary>
    /// Test site repository
    /// </summary>
    public class SiteRepositoryTests
    {
        private readonly Mock<IArtportalenDataService> _artportalenDataServiceMock;
        private readonly Mock<ILogger<SiteRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public SiteRepositoryTests()
        {
            _artportalenDataServiceMock = new Mock<IArtportalenDataService>();
            _loggerMock = new Mock<ILogger<SiteRepository>>();
        }

        /// <summary>
        /// Constructor tests
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new SiteRepository(
                _artportalenDataServiceMock.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new SiteRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("artportalenDataService");

            create = () => new SiteRepository(
                _artportalenDataServiceMock.Object,
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

            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<SiteEntity>(It.IsAny<string>(), null))
                .ReturnsAsync(projects);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var SiteRepository = new SiteRepository(
                _artportalenDataServiceMock.Object,
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
            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<SiteEntity>(It.IsAny<string>(), null))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var SiteRepository = new SiteRepository(
                _artportalenDataServiceMock.Object,
                _loggerMock.Object);

            var result = await SiteRepository.GetAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
    }
}
