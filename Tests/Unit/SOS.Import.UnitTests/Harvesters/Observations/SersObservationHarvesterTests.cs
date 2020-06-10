using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;

using SOS.Import.Harvesters.Observations;
using SOS.Import.Repositories.Destination.Sers.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Sers;
using Xunit;

namespace SOS.Import.UnitTests.Harvesters.Observations
{
    public class SersObservationHarvesterTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public SersObservationHarvesterTests()
        {
            _sersObservationVerbatimRepositoryMock = new Mock<ISersObservationVerbatimRepository>();
            _sersObservationServiceMock = new Mock<ISersObservationService>();
            _sersServiceConfiguration = new SersServiceConfiguration
                {MaxReturnedChangesInOnePage = 10, MaxNumberOfSightingsHarvested = 1};
            _loggerMock = new Mock<ILogger<SersObservationHarvester>>();
        }

        private readonly Mock<ISersObservationVerbatimRepository> _sersObservationVerbatimRepositoryMock;
        private readonly Mock<ISersObservationService> _sersObservationServiceMock;
        private readonly SersServiceConfiguration _sersServiceConfiguration;
        private readonly Mock<ILogger<SersObservationHarvester>> _loggerMock;

        private SersObservationHarvester TestObject => new SersObservationHarvester(
            _sersObservationServiceMock.Object,
            _sersObservationVerbatimRepositoryMock.Object,
            _sersServiceConfiguration,
            _loggerMock.Object);

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new SersObservationHarvester(
                null,
                _sersObservationVerbatimRepositoryMock.Object,
                _sersServiceConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("sersObservationService");

            create = () => new SersObservationHarvester(
                _sersObservationServiceMock.Object,
                null,
                _sersServiceConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should()
                .Be("sersObservationVerbatimRepository");

            create = () => new SersObservationHarvester(
                _sersObservationServiceMock.Object,
                _sersObservationVerbatimRepositoryMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("sersServiceConfiguration");

            create = () => new SersObservationHarvester(
                _sersObservationServiceMock.Object,
                _sersObservationVerbatimRepositoryMock.Object,
                _sersServiceConfiguration,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        ///     Test aggregation fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestSersAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _sersObservationServiceMock.Setup(cts => cts.GetAsync(It.IsAny<int>()))
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
        ///     Make a successful serss harvest
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestSersAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _sersObservationServiceMock.Setup(cts => cts.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(new XDocument());

            _sersObservationVerbatimRepositoryMock.Setup(tr => tr.DeleteCollectionAsync())
                .ReturnsAsync(true);
            _sersObservationVerbatimRepositoryMock.Setup(tr => tr.AddCollectionAsync())
                .ReturnsAsync(true);
            _sersObservationVerbatimRepositoryMock
                .Setup(tr => tr.AddManyAsync(It.IsAny<IEnumerable<SersObservationVerbatim>>()))
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