using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Jobs;
using SOS.Process.Managers.Interfaces;
using Xunit;

namespace SOS.Process.UnitTests.Jobs
{
    /// <summary>
    /// Tests for activate instance job
    /// </summary>
    public class CopyProviderDataJobTests
    {
        private readonly Mock<IInstanceManager> _instanceManagerMock;
        private readonly Mock<ILogger<CopyProviderDataJob>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public CopyProviderDataJobTests()
        {
            _instanceManagerMock = new Mock<IInstanceManager>();
            _loggerMock = new Mock<ILogger<CopyProviderDataJob>>();
        }

        // todo - delete test?
        // This test doesn't add any value to the unit test suite due to the following reasons:
        // 1) The constructor is always invoked by dependency injection, which means that this test adds no protection against regressions (bugs).
        // 2) This test tests the code implementation details and not the behavior of the system.
        //
        ///// <summary>
        ///// Test constructor
        ///// </summary>
        //[Fact]
        //public void ConstructorTest()
        //{
        //    new CopyProviderDataJob(
        //        _instanceManagerMock.Object,
        //        _loggerMock.Object).Should().NotBeNull();

        //    Action create = () => new CopyProviderDataJob(
        //        null,
        //        null,
        //        _loggerMock.Object);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("instanceFactory");


        //    create = () => new CopyProviderDataJob(
        //        _instanceManagerMock.Object,
        //        null,
        //       null);
        //    create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        //}

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
            _instanceManagerMock.Setup(r => r.CopyProviderDataAsync(It.IsAny<DataProvider>()))
                .ReturnsAsync(true);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var job = new CopyProviderDataJob(
                _instanceManagerMock.Object,
                null,
                _loggerMock.Object);

            var result = await job.RunAsync(It.IsAny<int>());
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
            var job = new CopyProviderDataJob(
                _instanceManagerMock.Object,
                null,
                _loggerMock.Object);

            var result = await job.RunAsync(It.IsAny<int>());
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
            _instanceManagerMock.Setup(r => r.CopyProviderDataAsync(It.IsAny<DataProvider>()))
                .ThrowsAsync(new Exception("Failed"));
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var job = new CopyProviderDataJob(
                _instanceManagerMock.Object,
                null,
                _loggerMock.Object);

            var result = await job.RunAsync(It.IsAny<int>());
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }
    }
}
