using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Harvesters.Observations;
using SOS.Import.Repositories.Destination.ClamPortal.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.ClamPortal;
using Xunit;

namespace SOS.Import.UnitTests.Harvesters.Observations
{
    public class ClamPortalObservationHarvesterTests
    {
        private readonly Mock<IClamObservationVerbatimRepository> _clamObservationVerbatimRepositoryMock;
        private readonly Mock<IClamObservationService> _clamObservationServiceMock;
        private readonly Mock<ILogger<ClamPortalObservationHarvester>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ClamPortalObservationHarvesterTests()
        {
            _clamObservationVerbatimRepositoryMock = new Mock<IClamObservationVerbatimRepository>();
            _clamObservationServiceMock = new Mock<IClamObservationService>();
            _loggerMock = new Mock<ILogger<ClamPortalObservationHarvester>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new ClamPortalObservationHarvester(
                _clamObservationVerbatimRepositoryMock.Object,
                _clamObservationServiceMock.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new ClamPortalObservationHarvester(
                null,
                _clamObservationServiceMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("clamObservationVerbatimRepository");

            create = () => new ClamPortalObservationHarvester(
                _clamObservationVerbatimRepositoryMock.Object,
              null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("clamObservationService");

            create = () => new ClamPortalObservationHarvester(
                _clamObservationVerbatimRepositoryMock.Object,
                _clamObservationServiceMock.Object,
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

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var clamPortalObservationHarvester = new ClamPortalObservationHarvester(
                _clamObservationVerbatimRepositoryMock.Object,
                _clamObservationServiceMock.Object,
                _loggerMock.Object);

            var result = await clamPortalObservationHarvester.HarvestClamsAsync(JobCancellationToken.Null);
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
            var clamPortalObservationHarvester = new ClamPortalObservationHarvester(
                _clamObservationVerbatimRepositoryMock.Object,
                _clamObservationServiceMock.Object,
                _loggerMock.Object);

            var result = await clamPortalObservationHarvester.HarvestClamsAsync(JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Failed);
        }
    }
}
