using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Configuration.Process;
using SOS.Process.Services;
using Xunit;

namespace SOS.Process.Test.Services
{
    /// <summary>
    /// Tests for sighting factory
    /// </summary>
    public class RuntimeServiceTests
    {
        private readonly RunTimeConfiguration _runTimeConfiguration;
        private readonly Mock<ILogger<RuntimeService>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public RuntimeServiceTests()
        {
            _runTimeConfiguration = new RunTimeConfiguration
            {
                Applications = new[]
                {
                    new ApplicationConfiguration
                    {
                        ServerName = "localhost",
                        ApplicationPool = "N/A",
                        SettingsFile = "appsettings.local.json",
                        Folder = @"C:\Users\msli0005\Source\repos\SOS\SOS.Search.Service\bin\Debug\netcoreapp3.0"
                    }
                }
            };

            _loggerMock = new Mock<ILogger<RuntimeService>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new RuntimeService(
                _runTimeConfiguration,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new RuntimeService(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("runTimeConfiguration");

            create = () => new RuntimeService(
                _runTimeConfiguration,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Make a successful test of processing
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task InitializeSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
           

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var runtimeService = new RuntimeService(
                _runTimeConfiguration,
                _loggerMock.Object);

            var result = runtimeService.Initialize();

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
        public async Task ToggleSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var runtimeService = new RuntimeService(
                _runTimeConfiguration,
                _loggerMock.Object);

            var initResult = runtimeService.Initialize();
            var activeInstance = runtimeService.ActiveInstance;
            await runtimeService.ToggleInstanceAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            initResult.Should().BeTrue();

            runtimeService.InactiveInstance.Should().Be(activeInstance);
        }
    }
}
