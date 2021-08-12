using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Constants;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Process.Managers;
using Xunit;

namespace SOS.Process.UnitTests.Managers
{
    /// <summary>
    ///     Tests for Instance manager
    /// </summary>
    public class InstanceManagerTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public InstanceManagerTests()
        {
            _processedObservationRepositoryMock = new Mock<IProcessedObservationRepository>();
            _processInfoRepositoryMock = new Mock<IProcessInfoRepository>();
            _loggerMock = new Mock<ILogger<InstanceManager>>();
        }

        private readonly Mock<IProcessedObservationRepository> _processedObservationRepositoryMock;
        private readonly Mock<IProcessInfoRepository> _processInfoRepositoryMock;
        private readonly Mock<ILogger<InstanceManager>> _loggerMock;

        /// <summary>
        ///     Test processing exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CopyProviderDataAsyncException()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processedObservationRepositoryMock.Setup(r => r.DeleteProviderDataAsync(It.IsAny<DataProvider>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception("Failed"));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var instanceManager = new InstanceManager(
                _processedObservationRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await instanceManager.CopyProviderDataAsync(new DataProvider
                {Id = 1, Identifier = DataProviderIdentifiers.Artportalen});
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }

        /// <summary>
        ///     Copy provider data fail
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

            var result = await instanceManager.CopyProviderDataAsync(new DataProvider
                {Id = 1, Identifier = DataProviderIdentifiers.Artportalen});
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }

        /// <summary>
        ///     Make a successful test of copy provider data
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CopyProviderDataAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processedObservationRepositoryMock.Setup(r => r.DeleteProviderDataAsync(It.IsAny<DataProvider>(), It.IsAny<bool>()))
                .ReturnsAsync(true);


            _processedObservationRepositoryMock.Setup(r => r.CopyProviderDataAsync(It.IsAny<DataProvider>(), It.IsAny<bool>()))
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

            var result = await instanceManager.CopyProviderDataAsync(new DataProvider
                {Id = 1, Identifier = DataProviderIdentifiers.Artportalen});
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }

        /// <summary>
        ///     Test processing exception
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

        /// <summary>
        ///     Copy provider data fail
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
        ///     Make a successful test of copy provider data
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
    }
}