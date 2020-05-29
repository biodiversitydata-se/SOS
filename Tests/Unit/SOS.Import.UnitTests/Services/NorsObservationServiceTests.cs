using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NorsService;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using Xunit;

namespace SOS.Import.UnitTests.Services
{
    public class NorsObservationServiceTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public NorsObservationServiceTests()
        {
            _speciesObservationChangeServiceMock = new Mock<ISpeciesObservationChangeService>();
            _norsServiceConfiguration = new NorsServiceConfiguration
                {MaxNumberOfSightingsHarvested = 10, MaxReturnedChangesInOnePage = 10};
            _loggerMock = new Mock<ILogger<NorsObservationService>>();
        }

        private readonly Mock<ISpeciesObservationChangeService> _speciesObservationChangeServiceMock;
        private readonly NorsServiceConfiguration _norsServiceConfiguration;
        private readonly Mock<ILogger<NorsObservationService>> _loggerMock;

        private NorsObservationService TestObject => new NorsObservationService(
            _speciesObservationChangeServiceMock.Object,
            _norsServiceConfiguration,
            _loggerMock.Object);

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new NorsObservationService(
                null,
                _norsServiceConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should()
                .Be("speciesObservationChangeServiceClient");

            create = () => new NorsObservationService(
                _speciesObservationChangeServiceMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("norsServiceConfiguration");

            create = () => new NorsObservationService(
                _speciesObservationChangeServiceMock.Object,
                _norsServiceConfiguration,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        ///     Get clams observations fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetNorsObservationsAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _speciesObservationChangeServiceMock.Setup(s => s.GetSpeciesObservationChangeAsSpeciesAsync(
                    It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>(),
                    It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<long>()))
                .Throws(new Exception("Exception"));
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () =>
            {
                await TestObject.GetAsync(It.IsAny<int>());
                ;
            };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<Exception>();
        }

        /// <summary>
        ///     Get clams observations success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetNorsObservationsAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _speciesObservationChangeServiceMock.Setup(s => s.GetSpeciesObservationChangeAsSpeciesAsync(
                    It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>(),
                    It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<long>()))
                .ReturnsAsync(new WebSpeciesObservationChange
                    {CreatedSpeciesObservations = new WebSpeciesObservation[0]});

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetAsync(It.IsAny<int>());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Item2.Count().Should().Be(0);
        }
    }
}