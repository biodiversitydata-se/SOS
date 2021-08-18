using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Process.Jobs;
using SOS.Process.Managers.Interfaces;
using Xunit;

namespace SOS.Process.UnitTests.Jobs
{
    /// <summary>
    ///     Tests for activate instance job
    /// </summary>
    public class CopyProviderDataJobTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public CopyProviderDataJobTests()
        {
            _instanceManagerMock = new Mock<IInstanceManager>();
            _dataProviderManagerMock = new Mock<IDataProviderManager>();
            _loggerMock = new Mock<ILogger<CopyProviderDataJob>>();
        }

        private readonly Mock<IInstanceManager> _instanceManagerMock;
        private readonly Mock<IDataProviderManager> _dataProviderManagerMock;
        private readonly Mock<ILogger<CopyProviderDataJob>> _loggerMock;

        /// <summary>
        ///     Test processing exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RunAsyncException()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _instanceManagerMock.Setup(r => r.CopyProviderDataAsync(It.IsAny<DataProvider>()))
                .ThrowsAsync(new Exception("Failed"));
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var job = new CopyProviderDataJob(
                _instanceManagerMock.Object,
                _dataProviderManagerMock.Object,
                _loggerMock.Object);

            Func<Task> act = async () => { await job.RunAsync(It.IsAny<int>()); };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            await act.Should().ThrowAsync<Exception>();
        }

        /// <summary>
        ///     Test processing fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RunAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var job = new CopyProviderDataJob(
                _instanceManagerMock.Object,
                _dataProviderManagerMock.Object,
                _loggerMock.Object);

            Func<Task> act = async () => { await job.RunAsync(It.IsAny<int>()); };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            await act.Should().ThrowAsync<Exception>();
        }

        /// <summary>
        ///     Make a successful test of processing
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RunAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _instanceManagerMock.Setup(r => r.CopyProviderDataAsync(It.IsAny<DataProvider>()))
                .ReturnsAsync(true);

            _dataProviderManagerMock.Setup(dpm => dpm.GetDataProviderByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new DataProvider());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var job = new CopyProviderDataJob(
                _instanceManagerMock.Object,
                _dataProviderManagerMock.Object,
                _loggerMock.Object);

            var result = await job.RunAsync(It.IsAny<int>());
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }
    }
}