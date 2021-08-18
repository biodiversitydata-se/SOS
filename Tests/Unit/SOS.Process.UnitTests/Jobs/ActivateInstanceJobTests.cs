using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Process.Jobs;
using SOS.Process.Managers.Interfaces;
using Xunit;

namespace SOS.Process.UnitTests.Jobs
{
    /// <summary>
    ///     Tests for activate instance job
    /// </summary>
    public class ActivateInstanceJobTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ActivateInstanceJobTests()
        {
            _instanceManagerMock = new Mock<IInstanceManager>();
            _loggerMock = new Mock<ILogger<ActivateInstanceJob>>();
        }

        private readonly Mock<IInstanceManager> _instanceManagerMock;
        private readonly Mock<ILogger<ActivateInstanceJob>> _loggerMock;

        /// <summary>
        ///     Test processing exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void RunAsyncException()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _instanceManagerMock.Setup(r => r.SetActiveInstanceAsync(It.IsAny<byte>()))
                .ThrowsAsync(new Exception("Failed"));
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var job = new ActivateInstanceJob(
                _instanceManagerMock.Object,
                _loggerMock.Object);

            Func<Task> act = async () => { await job.RunAsync(It.IsAny<byte>()); };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().ThrowAsync<Exception>();
        }

        /// <summary>
        ///     Test processing fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void RunAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var job = new ActivateInstanceJob(
                _instanceManagerMock.Object,
                _loggerMock.Object);

            Func<Task> act = async () => { await job.RunAsync(It.IsAny<byte>()); };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().ThrowAsync<Exception>();
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
            _instanceManagerMock.Setup(r => r.SetActiveInstanceAsync(It.IsAny<byte>()))
                .ReturnsAsync(true);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var job = new ActivateInstanceJob(
                _instanceManagerMock.Object,
                _loggerMock.Object);

            var result = await job.RunAsync(It.IsAny<byte>());
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }
    }
}