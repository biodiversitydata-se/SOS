using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
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
        ///     Test the constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new SightingRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("artportalenDataService");

            create = () => new SightingRepository(
                _artportalenDataServiceMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

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
            var span = new[]
            {
                new Tuple<int, int>(1, 2)
            };

            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<Tuple<int, int>>(It.IsAny<string>(), null, false))
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

        /// <summary>
        ///     Test get sightings project id's fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetProjectIdsAsyncException()
        {
            _artportalenDataServiceMock.Setup(spds =>
                    spds.QueryAsync<(int SightingId, int ProjectId)>(It.IsAny<string>(), null, false))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetSightingProjectIdsAsync(It.IsAny<IEnumerable<int>>());
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }

        /// <summary>
        ///     Test get sighting project id's success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetProjectIdsAsyncSuccess()
        {
            var projectIds = new List<(int SightingId, int ProjectId)>
            {
                (1, 1),
                (1, 2)
            };

            _artportalenDataServiceMock.Setup(spds =>
                    spds.QueryAsync<(int SightingId, int ProjectId)>(It.IsAny<string>(), null, false))
                .ReturnsAsync(projectIds);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetSightingProjectIdsAsync(It.IsAny<IEnumerable<int>>());
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }
    }
}