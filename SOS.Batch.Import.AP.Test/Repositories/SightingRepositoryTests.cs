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

namespace SOS.Batch.Import.AP.Test.Repositories
{
    /// <summary>
    /// Test sighting repository
    /// </summary>
    public class SightingRepositoryTests
    {
        private readonly Mock<ISpeciesPortalDataService> _speciesPortalDataServiceMock;
        private readonly Mock<ILogger<SightingRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public SightingRepositoryTests()
        {
            _speciesPortalDataServiceMock = new Mock<ISpeciesPortalDataService>();
            _loggerMock = new Mock<ILogger<SightingRepository>>();
        }

        /// <summary>
        /// Test the constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new SightingRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new SightingRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("speciesPortalDataService");

            create = () => new SightingRepository(
                _speciesPortalDataServiceMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Test get chunk of sightings success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetChunkAsyncSuccess()
        {
            IEnumerable<SightingEntity> projects = new []
            {
                    new SightingEntity { Id = 1 },
                    new SightingEntity { Id = 2 }
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<SightingEntity>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(projects);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var SightingRepository = new SightingRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await SightingRepository.GetChunkAsync(0, 10);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Test get chunk of sightings exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetChunkAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<SightingEntity>(It.IsAny<string>(), It.IsAny<object>()))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var SightingRepository = new SightingRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await SightingRepository.GetChunkAsync(0, 10);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }

        /// <summary>
        /// Test get sighting project id's success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetProjectIdsAsyncSuccess()
        {
            var projectIds = new []
            {
                new Tuple<int, int>(1, 1),
                new Tuple<int, int>(1, 2)
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<Tuple<int, int>>(It.IsAny<string>(), null))
                .ReturnsAsync(projectIds);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var SightingRepository = new SightingRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await SightingRepository.GetProjectIdsAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Test get sightings project id's fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetProjectIdsAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<Tuple<int, int>>(It.IsAny<string>(), null))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var SightingRepository = new SightingRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await SightingRepository.GetProjectIdsAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }

        /// <summary>
        /// Test get id span success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetIdSpanAsyncSuccess()
        {
            var span = new[]
            {
                new Tuple<int, int>(1, 2)
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<Tuple<int, int>>(It.IsAny<string>(), null))
                .ReturnsAsync(span);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var SightingRepository = new SightingRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await SightingRepository.GetIdSpanAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().Be(span[0]);
        }

        /// <summary>
        /// Test get id span fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetIdSpanAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<Tuple<int, int>>(It.IsAny<string>(), null))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var SightingRepository = new SightingRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await SightingRepository.GetIdSpanAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
    }
}
