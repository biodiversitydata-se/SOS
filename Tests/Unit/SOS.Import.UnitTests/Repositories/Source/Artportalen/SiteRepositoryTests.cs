using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen;
using SOS.Harvest.Services.Interfaces;
using Xunit;

namespace SOS.Import.UnitTests.Repositories.Source.Artportalen
{
    /// <summary>
    ///     Test site repository
    /// </summary>
    public class SiteRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public SiteRepositoryTests()
        {
            _artportalenDataServiceMock = new Mock<IArtportalenDataService>();
            _loggerMock = new Mock<ILogger<SiteRepository>>();
        }

        private readonly Mock<IArtportalenDataService> _artportalenDataServiceMock;
        private readonly Mock<ILogger<SiteRepository>> _loggerMock;

        private SiteRepository TestObject => new SiteRepository(
            _artportalenDataServiceMock.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test get sites fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsyncException()
        {
            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<SiteEntity>(It.IsAny<string>(), null, false))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetByIdsAsync(It.IsAny<IEnumerable<int>>());
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }

        /// <summary>
        ///     Test get sites success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsyncSuccess()
        {
            IEnumerable<SiteEntity> projects = new[]
            {
                new SiteEntity {Id = 1, Name = "Site 1"},
                new SiteEntity {Id = 2, Name = "Site 2"}
            };

            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<SiteEntity>(It.IsAny<string>(), It.IsAny<object>(), false))
                .ReturnsAsync(projects);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetByIdsAsync(new []{1,2});
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }
    }
}