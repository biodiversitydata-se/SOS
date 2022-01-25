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
using SOS.Lib.Models.Verbatim.Nors;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using Xunit;

namespace SOS.Import.UnitTests.Harvesters.Observations
{
    public class NorsObservationHarvesterTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public NorsObservationHarvesterTests()
        {
            _norsObservationVerbatimRepositoryMock = new Mock<INorsObservationVerbatimRepository>();
            _norsObservationServiceMock = new Mock<INorsObservationService>();
            _norsServiceConfiguration = new NorsServiceConfiguration
                { MaxNumberOfSightingsHarvested = 1 };
            _loggerMock = new Mock<ILogger<NorsObservationHarvester>>();
        }

        private readonly Mock<INorsObservationVerbatimRepository> _norsObservationVerbatimRepositoryMock;
        private readonly Mock<INorsObservationService> _norsObservationServiceMock;
        private readonly NorsServiceConfiguration _norsServiceConfiguration;
        private readonly Mock<ILogger<NorsObservationHarvester>> _loggerMock;

        private NorsObservationHarvester TestObject => new NorsObservationHarvester(
            _norsObservationServiceMock.Object,
            _norsObservationVerbatimRepositoryMock.Object,
            _norsServiceConfiguration,
            _loggerMock.Object);

        /// <summary>
        ///     Test aggregation fail
        /// </summary>
        /// <returns></returns>
        [Fact (Skip = "too slow for being a unit test. Todo - move to integration test.")]
        public async Task HarvestNorsAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _norsObservationServiceMock.Setup(cts => cts.GetAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("Fail"));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.HarvestObservationsAsync(JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Count.Should().Be(0);
        }

        /// <summary>
        ///     Make a successful nors harvest
        /// </summary>
        /// <returns></returns>
        [Fact(Skip = "too slow for being a unit test. Todo - move to integration test.")]
        public async Task HarvestNorsAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _norsObservationServiceMock.Setup(cts => cts.GetAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .ReturnsAsync(new XDocument());

            _norsObservationVerbatimRepositoryMock.Setup(tr => tr.DeleteCollectionAsync())
                .ReturnsAsync(true);
            _norsObservationVerbatimRepositoryMock.Setup(tr => tr.AddCollectionAsync())
                .ReturnsAsync(true);
            _norsObservationVerbatimRepositoryMock
                .Setup(tr => tr.AddManyAsync(It.IsAny<IEnumerable<NorsObservationVerbatim>>()))
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