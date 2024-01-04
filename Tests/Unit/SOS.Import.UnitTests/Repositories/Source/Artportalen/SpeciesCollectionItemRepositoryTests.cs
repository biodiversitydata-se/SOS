using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen;
using SOS.Harvest.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Import.UnitTests.Repositories.Source.Artportalen
{
    /// <summary>
    ///     Test site repository
    /// </summary>
    public class SpeciesCollectionItemRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public SpeciesCollectionItemRepositoryTests()
        {
            _artportalenDataServiceMock = new Mock<IArtportalenDataService>();
            _loggerMock = new Mock<ILogger<SpeciesCollectionItemRepository>>();
        }

        private readonly Mock<IArtportalenDataService> _artportalenDataServiceMock;
        private readonly Mock<ILogger<SpeciesCollectionItemRepository>> _loggerMock;

        private SpeciesCollectionItemRepository TestObject => new SpeciesCollectionItemRepository(
            _artportalenDataServiceMock.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test get sites fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsyncException()
        {
            _artportalenDataServiceMock
                .Setup(spds => spds.QueryAsync<SpeciesCollectionItemEntity>(It.IsAny<string>(), null, false))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetBySightingAsync(It.IsAny<IEnumerable<int>>());
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
    }
}