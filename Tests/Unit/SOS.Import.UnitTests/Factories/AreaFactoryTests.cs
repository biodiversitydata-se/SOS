using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Entities;
using SOS.Import.Factories;
using SOS.Import.Repositories.Destination.SpeciesPortal;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;
using Xunit;

namespace SOS.Import.UnitTests.Factories
{
    /// <summary>
    /// Tests for area factory
    /// </summary>
    public class AreaFactoryTests
    {
        private readonly Mock<IAreaRepository> _areaRepositoryMock;
        private readonly Mock<AreaVerbatimRepository> _areaVerbatimRepository;
        private readonly Mock<ILogger<AreaFactory>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public AreaFactoryTests()
        {
            _areaRepositoryMock = new Mock<IAreaRepository>();
            _areaVerbatimRepository = new Mock<AreaVerbatimRepository>();
            _loggerMock = new Mock<ILogger<AreaFactory>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new AreaFactory(
                _areaRepositoryMock.Object,
                _areaVerbatimRepository.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new AreaFactory(
                null,
                _areaVerbatimRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("areaRepository");

            create = () => new AreaFactory(
                _areaRepositoryMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("areaVerbatimRepository");

            create = () => new AreaFactory(
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
            var areaFactory = new AreaFactory(
                _areaRepositoryMock.Object,
                _areaVerbatimRepository.Object,
                _loggerMock.Object);

            var result = await areaFactory.HarvestAreasAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Success);
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
            var geoFactory = new AreaFactory(
                _areaRepositoryMock.Object,
                _areaVerbatimRepository.Object,
                _loggerMock.Object);

            var result = await geoFactory.HarvestAreasAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Failed);
        }
    }
}
