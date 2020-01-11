using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Process.Factories.Interfaces;
using SOS.Process.Jobs;
using Xunit;

namespace SOS.Process.Test.Jobs
{
    /// <summary>
    /// Tests for activate instance job
    /// </summary>
    public class ActivateInstanceJobTests
    {
        private readonly Mock<IInstanceFactory> _instanceFactoryMock;
        private readonly Mock<ILogger<ActivateInstanceJob>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ActivateInstanceJobTests()
        {
            _instanceFactoryMock = new Mock<IInstanceFactory>();
            _loggerMock = new Mock<ILogger<ActivateInstanceJob>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new ActivateInstanceJob(
                _instanceFactoryMock.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new ActivateInstanceJob(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("instanceFactory");

            
            create = () => new ActivateInstanceJob(
                _instanceFactoryMock.Object,
               null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Make a successful test of processing
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RunAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _instanceFactoryMock.Setup(r => r.SetActiveInstanceAsync(It.IsAny<byte>()))
                .ReturnsAsync(true);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var job = new ActivateInstanceJob(
                _instanceFactoryMock.Object,
                _loggerMock.Object);

            var result = await job.RunAsync(It.IsAny<byte>());
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }

        /// <summary>
        /// Test processing fail
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
            var job = new ActivateInstanceJob(
                _instanceFactoryMock.Object,
                _loggerMock.Object);

            var result = await job.RunAsync(It.IsAny<byte>());
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
        public async Task RunAsyncException()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _instanceFactoryMock.Setup(r => r.SetActiveInstanceAsync(It.IsAny<byte>()))
                .ThrowsAsync(new Exception("Failed"));
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var job = new ActivateInstanceJob(
                _instanceFactoryMock.Object,
                _loggerMock.Object);

            var result = await job.RunAsync(It.IsAny<byte>());
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }
    }
}
