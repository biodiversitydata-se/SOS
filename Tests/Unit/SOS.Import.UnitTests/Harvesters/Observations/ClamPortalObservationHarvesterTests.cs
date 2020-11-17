using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Harvesters.Observations;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using Xunit;

namespace SOS.Import.UnitTests.Harvesters.Observations
{
    public class ClamPortalObservationHarvesterTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ClamPortalObservationHarvesterTests()
        {
            _clamObservationVerbatimRepositoryMock = new Mock<IClamObservationVerbatimRepository>();
            _clamObservationServiceMock = new Mock<IClamObservationService>();
            _loggerMock = new Mock<ILogger<ClamPortalObservationHarvester>>();
        }

        private readonly Mock<IClamObservationVerbatimRepository> _clamObservationVerbatimRepositoryMock;
        private readonly Mock<IClamObservationService> _clamObservationServiceMock;
        private readonly Mock<ILogger<ClamPortalObservationHarvester>> _loggerMock;

        private ClamPortalObservationHarvester TestObject => new ClamPortalObservationHarvester(
            _clamObservationVerbatimRepositoryMock.Object,
            _clamObservationServiceMock.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test aggregation fail
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
            var result = await TestObject.HarvestClamsAsync(JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Failed);
        }

        /// <summary>
        ///     Make a successful clams harvest
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestClamsAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _clamObservationServiceMock.Setup(cts => cts.GetClamObservationsAsync())
                .ReturnsAsync(new[]
                {
                    new ClamObservationVerbatim
                    {
                        DyntaxaTaxonId = 100024
                    }
                });

            _clamObservationVerbatimRepositoryMock.Setup(tr => tr.DeleteCollectionAsync())
                .ReturnsAsync(true);
            _clamObservationVerbatimRepositoryMock.Setup(tr => tr.AddCollectionAsync())
                .ReturnsAsync(true);
            _clamObservationVerbatimRepositoryMock
                .Setup(tr => tr.AddManyAsync(It.IsAny<IEnumerable<ClamObservationVerbatim>>()))
                .ReturnsAsync(true);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.HarvestClamsAsync(JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Success);
        }
    }
}