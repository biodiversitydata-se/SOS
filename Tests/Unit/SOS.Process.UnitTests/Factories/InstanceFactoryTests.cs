using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Enums;
using SOS.Process.Factories;
using SOS.Process.Repositories.Destination.Interfaces;
using Xunit;

namespace SOS.Process.UnitTests.Factories
{
    /// <summary>
    /// Tests for sighting factory
    /// </summary>
    public class InstanceFactoryTests
    {
        private readonly Mock<IProcessedSightingRepository> _processedSightingRepositoryMock;
        private readonly Mock<IProcessInfoRepository> _processInfoRepositoryMock;
        private readonly Mock<ILogger<InstanceFactory>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public InstanceFactoryTests()
        {
            _processedSightingRepositoryMock = new Mock<IProcessedSightingRepository>();
            _processInfoRepositoryMock = new Mock<IProcessInfoRepository>();
            _loggerMock = new Mock<ILogger<InstanceFactory>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new InstanceFactory(
                _processedSightingRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new InstanceFactory(
                null,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("darwinCoreRepository");

            
            create = () => new InstanceFactory(
                _processedSightingRepositoryMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processInfoRepository");

            create = () => new InstanceFactory(
                _processedSightingRepositoryMock.Object,
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
            _processedSightingRepositoryMock.Setup(r => r.DeleteProviderDataAsync(It.IsAny<DataProvider>()))
                .ReturnsAsync(true);

            _processedSightingRepositoryMock.Setup(r => r.CopyProviderDataAsync(It.IsAny<DataProvider>()))
                .ReturnsAsync(true);
            
            _processInfoRepositoryMock.Setup(r => r.CopyProviderDataAsync(It.IsAny<DataProvider>()))
                .ReturnsAsync(true);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var factory = new InstanceFactory(
                _processedSightingRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await factory.CopyProviderDataAsync(DataProvider.Artdatabanken);
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
            var factory = new InstanceFactory(
                _processedSightingRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await factory.CopyProviderDataAsync(DataProvider.Artdatabanken);
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
            _processedSightingRepositoryMock.Setup(r => r.DeleteProviderDataAsync(It.IsAny<DataProvider>()))
                .ThrowsAsync(new Exception("Failed"));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var factory = new InstanceFactory(
                _processedSightingRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await factory.CopyProviderDataAsync(DataProvider.Artdatabanken);
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
            _processedSightingRepositoryMock.Setup(r => r.SetActiveInstanceAsync(It.IsAny<byte>()))
                .ReturnsAsync(true);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var factory = new InstanceFactory(
                _processedSightingRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await factory.SetActiveInstanceAsync(0);
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
            var factory = new InstanceFactory(
                _processedSightingRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await factory.SetActiveInstanceAsync(0);
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
            _processedSightingRepositoryMock.Setup(r => r.SetActiveInstanceAsync(It.IsAny<byte>()))
                .ThrowsAsync(new Exception("Failed"));

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var factory = new InstanceFactory(
                _processedSightingRepositoryMock.Object,
                _processInfoRepositoryMock.Object,
                _loggerMock.Object);

            var result = await factory.SetActiveInstanceAsync(0);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }
    }
}
