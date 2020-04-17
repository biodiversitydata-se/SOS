using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using NorsService;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Harvesters.Observations;
using SOS.Import.Repositories.Destination.Nors.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Nors;
using Xunit;

namespace SOS.Import.UnitTests.Harvesters.Observations
{
    public class NorsObservationHarvesterTests
    {
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
        /// Constructor
        /// </summary>
        public NorsObservationHarvesterTests()
        {
            _norsObservationVerbatimRepositoryMock = new Mock<INorsObservationVerbatimRepository>();
            _norsObservationServiceMock = new Mock<INorsObservationService>();
            _norsServiceConfiguration = new NorsServiceConfiguration { MaxReturnedChangesInOnePage = 10, MaxNumberOfSightingsHarvested = 1 };
            _loggerMock = new Mock<ILogger<NorsObservationHarvester>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new NorsObservationHarvester(
                null,
                _norsObservationVerbatimRepositoryMock.Object,
                _norsServiceConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("norsObservationService");

            create = () => new NorsObservationHarvester(
                _norsObservationServiceMock.Object,
               null,
                _norsServiceConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("norsObservationVerbatimRepository");

            create = () => new NorsObservationHarvester(
                _norsObservationServiceMock.Object,
                _norsObservationVerbatimRepositoryMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("norsServiceConfiguration");

            create = () => new NorsObservationHarvester(
                _norsObservationServiceMock.Object,
                _norsObservationVerbatimRepositoryMock.Object,
                _norsServiceConfiguration,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Make a successful norss harvest
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestNorsAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _norsObservationServiceMock.Setup(cts => cts.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<WebSpeciesObservation>());

            _norsObservationVerbatimRepositoryMock.Setup(tr => tr.DeleteCollectionAsync())
                .ReturnsAsync(true);
            _norsObservationVerbatimRepositoryMock.Setup(tr => tr.AddCollectionAsync())
                .ReturnsAsync(true);
            _norsObservationVerbatimRepositoryMock.Setup(tr => tr.AddManyAsync(It.IsAny<IEnumerable<NorsObservationVerbatim>>()))
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

        /// <summary>
        /// Test aggregation fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestNorsAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _norsObservationServiceMock.Setup(cts => cts.GetAsync(It.IsAny<int>()))
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
    }
}
