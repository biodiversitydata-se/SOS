using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Harvesters.Observations;
using SOS.Import.Repositories.Destination.Shark.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using Xunit;

namespace SOS.Import.UnitTests.Harvesters.Observations
{
    public class SharkObservationHarvesterTests
    {
        private readonly Mock<ISharkObservationVerbatimRepository> _sharkObservationVerbatimRepositoryMock;
        private readonly Mock<ISharkObservationService> _sharkObservationServiceMock;
        private readonly SharkServiceConfiguration _sharkServiceConfiguration;
        private readonly Mock<ILogger<SharkObservationHarvester>> _loggerMock;

        private SharkObservationHarvester TestObject => new SharkObservationHarvester(
            _sharkObservationServiceMock.Object,
            _sharkObservationVerbatimRepositoryMock.Object,
            _sharkServiceConfiguration,
            _loggerMock.Object);

        /// <summary>
        /// Constructor
        /// </summary>
        public SharkObservationHarvesterTests()
        {
            _sharkObservationVerbatimRepositoryMock = new Mock<ISharkObservationVerbatimRepository>();
            _sharkObservationServiceMock = new Mock<ISharkObservationService>();
            _sharkServiceConfiguration = new SharkServiceConfiguration { MaxNumberOfSightingsHarvested = 1 };
            _loggerMock = new Mock<ILogger<SharkObservationHarvester>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new SharkObservationHarvester(
                null,
                _sharkObservationVerbatimRepositoryMock.Object,
                _sharkServiceConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("sharkObservationService");

            create = () => new SharkObservationHarvester(
                _sharkObservationServiceMock.Object,
               null,
                _sharkServiceConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("sharkObservationVerbatimRepository");

            create = () => new SharkObservationHarvester(
                _sharkObservationServiceMock.Object,
                _sharkObservationVerbatimRepositoryMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("sharkServiceConfiguration");

            create = () => new SharkObservationHarvester(
                _sharkObservationServiceMock.Object,
                _sharkObservationVerbatimRepositoryMock.Object,
                _sharkServiceConfiguration,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Make a successful sharks harvest
        /// </summary>
        /// <returns></returns>
        /*   [Fact]
       public async Task HarvestSharkAsyncSuccess()
         {
             // -----------------------------------------------------------------------------------------------------------
             // Arrange
             //-----------------------------------------------------------------------------------------------------------
             _sharkObservationServiceMock.Setup(cts => cts.GetAsync(It.IsAny<string>()))
                 .ReturnsAsync(new SharkJsonFile{ Header = new [] {"id"} });
 
             _sharkObservationVerbatimRepositoryMock.Setup(tr => tr.DeleteCollectionAsync())
                 .ReturnsAsync(true);
             _sharkObservationVerbatimRepositoryMock.Setup(tr => tr.AddCollectionAsync())
                 .ReturnsAsync(true);
             _sharkObservationVerbatimRepositoryMock.Setup(tr => tr.AddManyAsync(It.IsAny<IEnumerable<SharkObservationVerbatim>>()))
                 .ReturnsAsync(true);
 
             //-----------------------------------------------------------------------------------------------------------
             // Act
             //-----------------------------------------------------------------------------------------------------------
             var result = await TestObject.HarvestObservationsAsync(JobCancellationToken.Null);
             //-----------------------------------------------------------------------------------------------------------
             // Assert
             //-----------------------------------------------------------------------------------------------------------
 
             result.Status.Should().Be(RunStatus.Success);
         }*/

        /// <summary>
        /// Test aggregation fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestSharkAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _sharkObservationServiceMock.Setup(cts => cts.GetAsync(It.IsAny<string>()))
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
