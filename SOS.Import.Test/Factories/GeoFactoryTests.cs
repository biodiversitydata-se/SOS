using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Entities;
using SOS.Import.Factories;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Import.Repositories.Destination.SpeciesPortal;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using Xunit;

namespace SOS.Import.Test.Factories
{
    /// <summary>
    /// Tests for sighting factory
    /// </summary>
    public class GeoFactoryTests
    {
        private readonly Mock<IAreaRepository> _areaRepositoryMock;
        private readonly Mock<AreaVerbatimRepository> _areaVerbatimRepository;
        private readonly Mock<ILogger<GeoFactory>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public GeoFactoryTests()
        {
            _areaRepositoryMock = new Mock<IAreaRepository>();
            _areaVerbatimRepository = new Mock<AreaVerbatimRepository>();
            _loggerMock = new Mock<ILogger<GeoFactory>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new GeoFactory(
                _areaRepositoryMock.Object,
                _areaVerbatimRepository.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new GeoFactory(
                null,
                _areaVerbatimRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("areaRepository");

            create = () => new GeoFactory(
                _areaRepositoryMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("areaVerbatimRepository");

            create = () => new GeoFactory(
                _areaRepositoryMock.Object,
                _areaVerbatimRepository.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");

        }

        /// <summary>
        /// Make a successful test of aggregation
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestAreasAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _areaRepositoryMock.Setup(mdr => mdr.GetAsync())
                .ReturnsAsync(new[] { new AreaEntity() { Id = 1, Name = "Sverige" } });

            _areaVerbatimRepository.Setup(tr => tr.DeleteCollectionAsync())
                .ReturnsAsync(true);
            _areaVerbatimRepository.Setup(tr => tr.AddCollectionAsync())
                .ReturnsAsync(true);
            _areaVerbatimRepository.Setup(tr => tr.AddManyAsync(It.IsAny<IEnumerable<Area>>()))
                .ReturnsAsync(true);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var geoFactory = new GeoFactory(
                _areaRepositoryMock.Object,
                _areaVerbatimRepository.Object,
                _loggerMock.Object);

            var result = await geoFactory.HarvestAreasAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }

        /// <summary>
        /// Test aggregation fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestAreasAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var geoFactory = new GeoFactory(
                _areaRepositoryMock.Object,
                _areaVerbatimRepository.Object,
                _loggerMock.Object);

            var result = await geoFactory.HarvestAreasAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }
    }
}
