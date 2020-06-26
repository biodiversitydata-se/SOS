using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MvmService;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using Xunit;

namespace SOS.Import.UnitTests.Services
{
    public class MvmObservationServiceTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public MvmObservationServiceTests()
        {
            _speciesObservationChangeServiceMock = new Mock<ISpeciesObservationChangeService>();
            _mvmServiceConfiguration = new MvmServiceConfiguration
                {MaxNumberOfSightingsHarvested = 10, MaxReturnedChangesInOnePage = 10};
            _loggerMock = new Mock<ILogger<MvmObservationService>>();
        }

        private readonly Mock<ISpeciesObservationChangeService> _speciesObservationChangeServiceMock;
        private readonly MvmServiceConfiguration _mvmServiceConfiguration;
        private readonly Mock<ILogger<MvmObservationService>> _loggerMock;

        private MvmObservationService TestObject => new MvmObservationService(
            _speciesObservationChangeServiceMock.Object,
            _mvmServiceConfiguration,
            _loggerMock.Object);

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new MvmObservationService(
                null,
                _mvmServiceConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should()
                .Be("speciesObservationChangeServiceClient");

            create = () => new MvmObservationService(
                _speciesObservationChangeServiceMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mvmServiceConfiguration");

            create = () => new MvmObservationService(
                _speciesObservationChangeServiceMock.Object,
                _mvmServiceConfiguration,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        ///     Get clams observations fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void GetMvmObservationsAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            /*  _speciesObservationChangeServiceMock.Setup(s => s.GetSpeciesObservationChangeAsSpeciesAsync(
                      It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>(),
                      It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<long>()))
                  .Throws(new Exception("Exception"));
              //-----------------------------------------------------------------------------------------------------------
              // Act
              //-----------------------------------------------------------------------------------------------------------
              Func<Task> act = async () => { await TestObject.GetAsync(It.IsAny<int>()); ; };
              
              //-----------------------------------------------------------------------------------------------------------
              // Assert
              //-----------------------------------------------------------------------------------------------------------
              act.Should().Throw<Exception>();*/
        }

        /// <summary>
        ///     Get clams observations success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void GetMvmObservationsAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            /*_speciesObservationChangeServiceMock.Setup(s => s.GetSpeciesObservationChangeAsSpeciesAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>(),
                    It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<long>()))
                .ReturnsAsync(new WebSpeciesObservationChange{CreatedSpeciesObservations = new WebSpeciesObservation[0]});

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetAsync(It.IsAny<int>());
          
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Count().Should().Be(0);*/
        }
    }
}