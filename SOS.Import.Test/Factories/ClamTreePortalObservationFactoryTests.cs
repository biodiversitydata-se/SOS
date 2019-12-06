using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Factories;
using SOS.Import.Repositories.Destination.ClamTreePortal.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.ClamTreePortal;
using Xunit;

namespace SOS.Import.Test.Factories
{
    public class ClamTreePortalObservationFactoryTests
    {
        private readonly Mock<IClamObservationVerbatimRepository> _clamObservationVerbatimRepositoryMock;
        private readonly Mock<ITreeObservationVerbatimRepository> _treeObservationVerbatimRepositoryMock;
        private readonly Mock<IClamTreeObservationService> _clamTreeObservationServiceMock;
        private readonly Mock<IHarvestInfoRepository> _harvestInfoRepositoryMock;
        private readonly Mock<ILogger<ClamTreePortalObservationFactory>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ClamTreePortalObservationFactoryTests()
        {
            _clamObservationVerbatimRepositoryMock = new Mock<IClamObservationVerbatimRepository>();
            _treeObservationVerbatimRepositoryMock = new Mock<ITreeObservationVerbatimRepository>();
            _clamTreeObservationServiceMock = new Mock<IClamTreeObservationService>();
            _harvestInfoRepositoryMock = new Mock<IHarvestInfoRepository>();
            _loggerMock = new Mock<ILogger<ClamTreePortalObservationFactory>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new ClamTreePortalObservationFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _treeObservationVerbatimRepositoryMock.Object,
                _clamTreeObservationServiceMock.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new ClamTreePortalObservationFactory(
                null,
                _treeObservationVerbatimRepositoryMock.Object,
                _clamTreeObservationServiceMock.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("clamObservationVerbatimRepository");

            create = () => new ClamTreePortalObservationFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                null,
                _clamTreeObservationServiceMock.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("treeObservationVerbatimRepository");

            create = () => new ClamTreePortalObservationFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _treeObservationVerbatimRepositoryMock.Object,
                null,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("clamTreeObservationService");

            create = () => new ClamTreePortalObservationFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _treeObservationVerbatimRepositoryMock.Object,
                _clamTreeObservationServiceMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("harvestInfoRepository");

            create = () => new ClamTreePortalObservationFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _treeObservationVerbatimRepositoryMock.Object,
                _clamTreeObservationServiceMock.Object,
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
            _clamTreeObservationServiceMock.Setup(cts => cts.GetClamObservationsAsync())
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
                hir.UpdateHarvestInfoAsync(It.IsAny<string>(), DataProviderId.ClamAndTreePortal, It.IsAny<DateTime>(), It.IsAny<DateTime>(),It.IsAny<int>()))
                .ReturnsAsync(true);
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var sightingFactory = new ClamTreePortalObservationFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _treeObservationVerbatimRepositoryMock.Object,
                _clamTreeObservationServiceMock.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await sightingFactory.HarvestClamsAsync();
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
            _clamTreeObservationServiceMock.Setup(cts => cts.GetClamObservationsAsync())
                .ThrowsAsync(new Exception("Fail"));
            
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var sightingFactory = new ClamTreePortalObservationFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _treeObservationVerbatimRepositoryMock.Object,
                _clamTreeObservationServiceMock.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await sightingFactory.HarvestClamsAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }

        /// <summary>
        /// Make a successful clams harvest
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestTreesAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _clamTreeObservationServiceMock.Setup(cts => cts.GetTreeObservationsAsync(1, It.IsAny<int>()))
                .ReturnsAsync(new[] { new TreeObservationVerbatim
                {
                    DyntaxaTaxonId = 100024
                }  });

            _treeObservationVerbatimRepositoryMock.Setup(tr => tr.DeleteCollectionAsync())
                .ReturnsAsync(true);
            _treeObservationVerbatimRepositoryMock.Setup(tr => tr.AddCollectionAsync())
                .ReturnsAsync(true);
            _treeObservationVerbatimRepositoryMock.Setup(tr => tr.AddManyAsync(It.IsAny<IEnumerable<TreeObservationVerbatim>>()))
                .ReturnsAsync(true);
            _harvestInfoRepositoryMock.Setup(hir =>
                    hir.UpdateHarvestInfoAsync(It.IsAny<string>(), DataProviderId.ClamAndTreePortal, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var sightingFactory = new ClamTreePortalObservationFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _treeObservationVerbatimRepositoryMock.Object,
                _clamTreeObservationServiceMock.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await sightingFactory.HarvestTreesAsync(JobCancellationToken.Null);
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
        public async Task HarvestTreesAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _clamTreeObservationServiceMock.Setup(cts => cts.GetTreeObservationsAsync(1, It.IsAny<int>()))
                .ThrowsAsync(new Exception("Fail"));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var sightingFactory = new ClamTreePortalObservationFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _treeObservationVerbatimRepositoryMock.Object,
                _clamTreeObservationServiceMock.Object,
                _harvestInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await sightingFactory.HarvestTreesAsync(JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }
    }
}
