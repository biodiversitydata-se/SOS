using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Factories;
using SOS.Import.Repositories.Destination.ClamPortal.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.ClamPortal;
using Xunit;

namespace SOS.Import.Test.Factories
{
    public class ClamPortalObservationFactoryTests
    {
        private readonly Mock<IClamObservationVerbatimRepository> _clamObservationVerbatimRepositoryMock;
        private readonly Mock<IClamObservationService> _clamObservationServiceMock;
        private readonly Mock<IHarvestInfoRepository> _harvestInfoRepositoryMock;
        private readonly Mock<ILogger<ClamPortalObservationFactory>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ClamPortalObservationFactoryTests()
        {
            _clamObservationVerbatimRepositoryMock = new Mock<IClamObservationVerbatimRepository>();
            _clamObservationServiceMock = new Mock<IClamObservationService>();
            _harvestInfoRepositoryMock = new Mock<IHarvestInfoRepository>();
            _loggerMock = new Mock<ILogger<ClamPortalObservationFactory>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new ClamPortalObservationFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _clamObservationServiceMock.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new ClamPortalObservationFactory(
                null,
                _clamObservationServiceMock.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("clamObservationVerbatimRepository");

            create = () => new ClamPortalObservationFactory(
                _clamObservationVerbatimRepositoryMock.Object,
              null,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("clamObservationService");

            create = () => new ClamPortalObservationFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _clamObservationServiceMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("harvestInfoRepository");

            create = () => new ClamPortalObservationFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _clamObservationServiceMock.Object,
                _harvestInfoRepositoryMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Make a successful clams harvest
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestClamsAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _clamObservationServiceMock.Setup(cts => cts.GetClamObservationsAsync())
                .ReturnsAsync(new[] { new ClamObservationVerbatim
                {
                    DyntaxaTaxonId = 100024
                }  });

            _clamObservationVerbatimRepositoryMock.Setup(tr => tr.DeleteCollectionAsync())
                .ReturnsAsync(true);
            _clamObservationVerbatimRepositoryMock.Setup(tr => tr.AddCollectionAsync())
                .ReturnsAsync(true);
            _clamObservationVerbatimRepositoryMock.Setup(tr => tr.AddManyAsync(It.IsAny<IEnumerable<ClamObservationVerbatim>>()))
                .ReturnsAsync(true);

            _harvestInfoRepositoryMock.Setup(hir =>
                hir.UpdateHarvestInfoAsync(It.IsAny<string>(), DataProvider.ClamPortal, It.IsAny<DateTime>(), It.IsAny<DateTime>(),It.IsAny<int>()))
                .ReturnsAsync(true);
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var sightingFactory = new ClamPortalObservationFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _clamObservationServiceMock.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await sightingFactory.HarvestClamsAsync(JobCancellationToken.Null);
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
        public async Task HarvestClamsAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _clamObservationServiceMock.Setup(cts => cts.GetClamObservationsAsync())
                .ThrowsAsync(new Exception("Fail"));
            
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var sightingFactory = new ClamPortalObservationFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _clamObservationServiceMock.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await sightingFactory.HarvestClamsAsync(JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }
    }
}
