﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SersService;
using SOS.Import.Services;
using SOS.Lib.Configuration.Import;
using Xunit;

namespace SOS.Import.UnitTests.Services
{
    public class SersObservationServiceTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public SersObservationServiceTests()
        {
            _speciesObservationChangeServiceMock = new Mock<ISpeciesObservationChangeService>();
            _sersServiceConfiguration = new SersServiceConfiguration
                {MaxNumberOfSightingsHarvested = 10, MaxReturnedChangesInOnePage = 10};
            _loggerMock = new Mock<ILogger<SersObservationService>>();
        }

        private readonly Mock<ISpeciesObservationChangeService> _speciesObservationChangeServiceMock;
        private readonly SersServiceConfiguration _sersServiceConfiguration;
        private readonly Mock<ILogger<SersObservationService>> _loggerMock;

        private SersObservationService TestObject => new SersObservationService(
            _speciesObservationChangeServiceMock.Object,
            _sersServiceConfiguration,
            _loggerMock.Object);

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new SersObservationService(
                null,
                _sersServiceConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should()
                .Be("speciesObservationChangeServiceClient");

            create = () => new SersObservationService(
                _speciesObservationChangeServiceMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("sersServiceConfiguration");

            create = () => new SersObservationService(
                _speciesObservationChangeServiceMock.Object,
                _sersServiceConfiguration,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        ///     Get clams observations fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetSersObservationsAsyncFail()
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
        public async Task GetSersObservationsAsyncSuccess()
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