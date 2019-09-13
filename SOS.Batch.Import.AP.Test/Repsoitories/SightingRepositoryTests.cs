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
    public class SightingRepositoryTests
    {
        private Mock<IDbConnection> _connection;
        private Mock<ISpeciesPortalDataService> _speciesPortalDataServiceMock;
        private Mock<ILogger<SightingRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public SightingRepositoryTests()
        {
            _connection = new Mock<IDbConnection>();
            _speciesPortalDataServiceMock = new Mock<ISpeciesPortalDataService>();
            _loggerMock = new Mock<ILogger<SightingRepository>>();
        }

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

        [Fact]
        public async Task GetChunkAsyncSuccess()
        {
            IEnumerable<SightingEntity> projects = new []
            {
                    new SightingEntity { Id = 1 },
                    new SightingEntity { Id = 2 }
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<SightingEntity>(It.IsAny<string>()))
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

        [Fact]
        public async Task GetChunkAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<SightingEntity>(It.IsAny<string>()))
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

        [Fact]
        public async Task GetProjectIdsAsyncSuccess()
        {
            var projectIds = new []
            {
                new Tuple<int, int>(1, 1),
                new Tuple<int, int>(1, 2)
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<Tuple<int, int>>(It.IsAny<string>()))
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

        [Fact]
        public async Task GetProjectIdsAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<Tuple<int, int>>(It.IsAny<string>()))
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

        [Fact]
        public async Task GetIdSpanAsyncSuccess()
        {
            var span = new[]
            {
                new Tuple<int, int>(1, 2)
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<Tuple<int, int>>(It.IsAny<string>()))
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

        [Fact]
        public async Task GetIdSpanAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<Tuple<int, int>>(It.IsAny<string>()))
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
