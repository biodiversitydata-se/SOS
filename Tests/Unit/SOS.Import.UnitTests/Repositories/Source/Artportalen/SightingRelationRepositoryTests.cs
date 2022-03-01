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
    ///     SightingRelation tests
    /// </summary>
    public class SightingRelationRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public SightingRelationRepositoryTests()
        {
            _artportalenDataServiceMock = new Mock<IArtportalenDataService>();
            _loggerMock = new Mock<ILogger<SightingRelationRepository>>();
        }

        private readonly Mock<IArtportalenDataService> _artportalenDataServiceMock;
        private readonly Mock<ILogger<SightingRelationRepository>> _loggerMock;

        private SightingRelationRepository TestObject => new SightingRelationRepository(
            _artportalenDataServiceMock.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test get projects fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsyncException()
        {
            _artportalenDataServiceMock.Setup(spds =>
                    spds.QueryAsync<SightingRelationEntity>(It.IsAny<string>(), It.IsAny<object>(), false))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetAsync(new[] {1, 2});
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }


        /// <summary>
        ///     Test get projects success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsyncSuccess()
        {
            IEnumerable<SightingRelationEntity> sightingRelations = new[]
            {
                new SightingRelationEntity {Id = 1},
                new SightingRelationEntity {Id = 2}
            };

            _artportalenDataServiceMock.Setup(spds =>
                    spds.QueryAsync<SightingRelationEntity>(It.IsAny<string>(), It.IsAny<object>(), false))
                .ReturnsAsync(sightingRelations);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetAsync(new[] {1, 2});
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }
    }
}