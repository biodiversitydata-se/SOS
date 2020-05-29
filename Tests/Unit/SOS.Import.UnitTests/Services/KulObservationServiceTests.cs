using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using KulService;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using Xunit;

namespace SOS.Import.UnitTests.Services
{
    public class KulObservationServiceTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public KulObservationServiceTests()
        {
            _speciesObservationChangeServiceMock = new Mock<ISpeciesObservationChangeService>();
            _clamServiceConfiguration = new KulServiceConfiguration
            {
                MaxNumberOfSightingsHarvested = 10, MaxReturnedChangesInOnePage = 10,
                StartHarvestYear = DateTime.Now.Year
            };
            _loggerMock = new Mock<ILogger<KulObservationService>>();
        }

        private readonly Mock<ISpeciesObservationChangeService> _speciesObservationChangeServiceMock;
        private readonly KulServiceConfiguration _clamServiceConfiguration;
        private readonly Mock<ILogger<KulObservationService>> _loggerMock;

        private KulObservationService TestObject => new KulObservationService(
            _speciesObservationChangeServiceMock.Object,
            _clamServiceConfiguration,
            _loggerMock.Object);

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new KulObservationService(
                null,
                _clamServiceConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should()
                .Be("speciesObservationChangeServiceClient");

            create = () => new KulObservationService(
                _speciesObservationChangeServiceMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("kulServiceConfiguration");

            create = () => new KulObservationService(
                _speciesObservationChangeServiceMock.Object,
                _clamServiceConfiguration,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        ///     Get clams observations fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetKulObservationsAsyncFail()
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
            Func<Task> act = async () => { await TestObject.GetAsync(DateTime.Now, DateTime.Now.AddDays(1)); };

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
        public async Task GetKulObservationsAsyncSuccess()
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
            var result = await TestObject.GetAsync(DateTime.Now, DateTime.Now.AddDays(1));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Count().Should().Be(0);
        }
    }
}