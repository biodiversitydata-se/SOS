using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Repositories.Source.Artportalen;
using SOS.Import.Services.Interfaces;
using Xunit;

namespace SOS.Import.UnitTests.Repositories.Source.Artportalen
{
    /// <summary>
    ///     Test sighting repository
    /// </summary>
    public class SightingRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public SightingRepositoryTests()
        {
            _artportalenDataServiceMock = new Mock<IArtportalenDataService>();
            _loggerMock = new Mock<ILogger<SightingRepository>>();
        }

        private readonly Mock<IArtportalenDataService> _artportalenDataServiceMock;
        private readonly Mock<ILogger<SightingRepository>> _loggerMock;

        private SightingRepository TestObject => new SightingRepository(
            _artportalenDataServiceMock.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test get chunk of sightings exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetChunkAsyncException()
        {
            _artportalenDataServiceMock
                .Setup(spds => spds.QueryAsync<SightingEntity>(It.IsAny<string>(), It.IsAny<object>(), false))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetChunkAsync(0, 10);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }

        /// <summary>
        ///     Test get chunk of sightings success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetChunkAsyncSuccess()
        {
            IEnumerable<SightingEntity> projects = new[]
            {
                new SightingEntity {Id = 1},
                new SightingEntity {Id = 2}
            };

            _artportalenDataServiceMock
                .Setup(spds => spds.QueryAsync<SightingEntity>(It.IsAny<string>(), It.IsAny<object>(), false))
                .ReturnsAsync(projects);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetChunkAsync(0, 10);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        ///     Test get id span fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetIdSpanAsyncException()
        {
            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<Tuple<int, int>>(It.IsAny<string>(), null, false))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetIdSpanAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }

        /// <summary>
        ///     Test get id span success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetIdSpanAsyncSuccess()
        {
            (int minId, int maxId)[] span = new [] { (1, 2) };

            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<(int minId, int maxId)>(It.IsAny<string>(), null, false))
                .ReturnsAsync(span);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetIdSpanAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().Be(span[0]);
        }
    }
}