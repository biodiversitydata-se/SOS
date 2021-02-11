using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NetTopologySuite.Geometries;
using SOS.Import.Entities.Artportalen;
using SOS.Import.Harvesters;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using Xunit;

namespace SOS.Import.UnitTests.Harvesters
{
    /// <summary>
    ///     Tests for area harvester
    /// </summary>
    public class AreaHarvesterTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public AreaHarvesterTests()
        {
            _areaRepositoryMock = new Mock<Import.Repositories.Source.Artportalen.Interfaces.IAreaRepository>();
            _areaProcessedRepository = new Mock<IAreaRepository>();
            _areaHelperMock = new Mock<IAreaHelper>();
            _geoRegionApiServiceMock = new Mock<IGeoRegionApiService>();
            _loggerMock = new Mock<ILogger<AreaHarvester>>();
        }

        private readonly Mock<Import.Repositories.Source.Artportalen.Interfaces.IAreaRepository> _areaRepositoryMock;
        private readonly Mock<IAreaRepository> _areaProcessedRepository;
        private readonly Mock<IAreaHelper> _areaHelperMock;
        private readonly Mock<IGeoRegionApiService> _geoRegionApiServiceMock;
        private readonly Mock<ILogger<AreaHarvester>> _loggerMock;

        private AreaHarvester TestObject => new AreaHarvester(
            _areaRepositoryMock.Object,
            _areaProcessedRepository.Object,
            _areaHelperMock.Object,
            _geoRegionApiServiceMock.Object,
            new AreaHarvestConfiguration(), 
            _loggerMock.Object);

        /// <summary>
        ///     Test aggregation fail
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
            var result = await TestObject.HarvestAreasAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Failed);
        }

        /// <summary>
        ///     Make a successful test of aggregation
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestAreasAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _areaRepositoryMock.Setup(mdr => mdr.GetAsync())
                .ReturnsAsync(new[] {new AreaEntity {FeatureId = "1", Name = "Sverige", PolygonWKT = "POLYGON ((30 10, 40 40, 20 40, 10 20, 30 10))" } });

            _areaProcessedRepository.Setup(tr => tr.DeleteCollectionAsync())
                .ReturnsAsync(true);
            _areaProcessedRepository.Setup(tr => tr.AddCollectionAsync())
                .ReturnsAsync(true);
            _areaProcessedRepository.Setup(tr => tr.AddManyAsync(It.IsAny<IEnumerable<Area>>()))
                .ReturnsAsync(true);
            _areaProcessedRepository.Setup(tr => tr.StoreGeometriesAsync(It.IsAny<IDictionary<string, Geometry>>()))
                .ReturnsAsync(true);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.HarvestAreasAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Status.Should().Be(RunStatus.Success);
        }
    }
}