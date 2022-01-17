using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Harvesters.Observations;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Kul;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using Xunit;

namespace SOS.Import.UnitTests.Harvesters.Observations
{
    public class KulObservationHarvesterTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public KulObservationHarvesterTests()
        {
            _kulObservationVerbatimRepositoryMock = new Mock<IKulObservationVerbatimRepository>();
            _kulObservationServiceMock = new Mock<IKulObservationService>();
            _kulServiceConfiguration = new KulServiceConfiguration {StartHarvestYear = DateTime.Now.Year};
            _loggerMock = new Mock<ILogger<KulObservationHarvester>>();
        }

        private readonly Mock<IKulObservationVerbatimRepository> _kulObservationVerbatimRepositoryMock;
        private readonly Mock<IKulObservationService> _kulObservationServiceMock;
        private readonly KulServiceConfiguration _kulServiceConfiguration;
        private readonly Mock<ILogger<KulObservationHarvester>> _loggerMock;

        private KulObservationHarvester TestObject => new KulObservationHarvester(
            _kulObservationServiceMock.Object,
            _kulObservationVerbatimRepositoryMock.Object,
            _kulServiceConfiguration,
            _loggerMock.Object);

        /// <summary>
        ///     Test aggregation fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestKulsAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _kulObservationServiceMock.Setup(cts => cts.GetAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<long>()))
                .ThrowsAsync(new Exception("Fail"));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.HarvestObservationsAsync(JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Failed);
        }

        /// <summary>
        ///     Make a successful kuls harvest
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestKulsAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _kulObservationServiceMock.Setup(cts => cts.GetAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<long>()))
                .ReturnsAsync(new XDocument());

            _kulObservationVerbatimRepositoryMock.Setup(tr => tr.DeleteCollectionAsync())
                .ReturnsAsync(true);
            _kulObservationVerbatimRepositoryMock.Setup(tr => tr.AddCollectionAsync())
                .ReturnsAsync(true);
            _kulObservationVerbatimRepositoryMock
                .Setup(tr => tr.AddManyAsync(It.IsAny<IEnumerable<KulObservationVerbatim>>()))
                .ReturnsAsync(true);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.HarvestObservationsAsync(JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Success);
        }
    }
}