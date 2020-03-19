using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Enums;
using SOS.Process.Managers;
using SOS.Process.Repositories.Destination.Interfaces;
using Xunit;

namespace SOS.Process.UnitTests.Managers
{
    /// <summary>
    /// Tests for Instance manager
    /// </summary>
    public class InstanceManagerTests
    {
        private readonly Mock<IProcessedObservationRepository> _processedObservationRepositoryMock;
        private readonly Mock<IProcessInfoRepository> _processInfoRepositoryMock;
        private readonly Mock<ILogger<InstanceManager>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public InstanceManagerTests()
        {
            _processedObservationRepositoryMock = new Mock<IProcessedObservationRepository>();
            _processInfoRepositoryMock = new Mock<IProcessInfoRepository>();
            _loggerMock = new Mock<ILogger<InstanceManager>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new InstanceManager(
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new InstanceManager(
                null,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("darwinCoreRepository");

            
            create = () => new InstanceManager(
                _processedObservationRepositoryMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processInfoRepository");

            create = () => new InstanceManager(
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Make a successful test of copy provider data
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CopyProviderDataAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processedObservationRepositoryMock.Setup(r => r.DeleteProviderDataAsync(It.IsAny<DataProvider>()))
                .ReturnsAsync(true);

            _processedObservationRepositoryMock.Setup(r => r.CopyProviderDataAsync(It.IsAny<DataProvider>()))
                .ReturnsAsync(true);
            
            _processInfoRepositoryMock.Setup(r => r.CopyProviderDataAsync(It.IsAny<DataProvider>()))
                .ReturnsAsync(true);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var instanceManager = new InstanceManager(
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await instanceManager.CopyProviderDataAsync(DataProvider.Artportalen);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }

        /// <summary>
        /// Copy provider data fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CopyProviderDataAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var instanceManager = new InstanceManager(
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await instanceManager.CopyProviderDataAsync(DataProvider.Artportalen);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }

        /// <summary>
        /// Test processing exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CopyProviderDataAsyncException()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processedObservationRepositoryMock.Setup(r => r.DeleteProviderDataAsync(It.IsAny<DataProvider>()))
                .ThrowsAsync(new Exception("Failed"));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var instanceManager = new InstanceManager(
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await instanceManager.CopyProviderDataAsync(DataProvider.Artportalen);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }

        /// <summary>
        /// Make a successful test of copy provider data
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SetActiveInstanceAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processedObservationRepositoryMock.Setup(r => r.SetActiveInstanceAsync(It.IsAny<byte>()))
                .ReturnsAsync(true);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var instanceManager = new InstanceManager(
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await instanceManager.SetActiveInstanceAsync(0);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }

        /// <summary>
        /// Copy provider data fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SetActiveInstanceAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var instanceManager = new InstanceManager(
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await instanceManager.SetActiveInstanceAsync(0);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }

        /// <summary>
        /// Test processing exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SetActiveInstanceAsyncException()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processedObservationRepositoryMock.Setup(r => r.SetActiveInstanceAsync(It.IsAny<byte>()))
                .ThrowsAsync(new Exception("Failed"));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var instanceManager = new InstanceManager(
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await instanceManager.SetActiveInstanceAsync(0);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }
    }
}
