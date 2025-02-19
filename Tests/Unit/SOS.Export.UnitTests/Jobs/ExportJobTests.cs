using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.Jobs;
using SOS.Export.Managers.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Export.UnitTests.Jobs
{
    /// <summary>
    ///     Tests for observation manager
    /// </summary>
    public class ExportAndSendJobTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ExportAndSendJobTests()
        {
            _observationManager = new Mock<IObservationManager>();
            _userExportRepository = new Mock<IUserExportRepository>();
            _cryptoService = new Mock<ICryptoService>();
            _loggerMock = new Mock<ILogger<ExportAndSendJob>>();
        }

        private readonly Mock<IObservationManager> _observationManager;
        private readonly Mock<IUserExportRepository> _userExportRepository;
        private readonly Mock<ICryptoService> _cryptoService;
        private readonly Mock<ILogger<ExportAndSendJob>> _loggerMock;

        /// <summary>
        ///     Return object to be tested
        /// </summary>
        private ExportAndSendJob TestObject => new ExportAndSendJob(
            _observationManager.Object,
            _userExportRepository.Object,
            _cryptoService.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test run fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task RunAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _observationManager
                .Setup(blss => blss
                    .ExportAndSendAsync(
                        It.IsAny<int>(),
                        It.IsAny<string>(),
                        It.IsAny<SearchFilter>(),
                        It.IsAny<string>(),
                        "",
                        ExportFormat.DwC,
                        "en-GB", false,
                        PropertyLabelType.PropertyName,
                        false,
                        It.IsAny<bool>(),
                        It.IsAny<bool>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        JobCancellationToken.Null)
                )
                .ReturnsAsync(new Models.ZendTo.ZendToResponse());
            _userExportRepository.Setup(uer => uer.GetAsync(It.IsAny<int>())).ReturnsAsync(new UserExport());
            _userExportRepository.Setup(uer => uer.AddOrUpdateAsync(It.IsAny<UserExport>())).ReturnsAsync(true);
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observationManager = TestObject;

            Func<Task> act = async () =>
            {
                await observationManager.RunAsync(new SearchFilter(0), 0, "", null, "", ExportFormat.DwC, "en-GB", false, PropertyLabelType.PropertyName, false, false, false, null, false, null, JobCancellationToken.Null);
            };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            await act.Should().ThrowAsync<Exception>();
        }

        /// <summary>
        ///     Make a successful test of export
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task RunAsyncSucess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _observationManager
                .Setup(blss => blss
                    .ExportAndSendAsync(
                        It.IsAny<int>(),
                        It.IsAny<string>(),
                        It.IsAny<SearchFilter>(),
                        It.IsAny<string>(),
                        "",
                        ExportFormat.DwC,
                        "en-GB",
                        false,
                        PropertyLabelType.
                        PropertyName,
                        false,
                        It.IsAny<bool>(),
                        It.IsAny<bool>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        JobCancellationToken.Null)
                )
                .ReturnsAsync(new Models.ZendTo.ZendToResponse { Status = "OK" });

            _userExportRepository.Setup(uer => uer.GetAsync(It.IsAny<int>())).ReturnsAsync(new UserExport());
            _userExportRepository.Setup(uer => uer.AddOrUpdateAsync(It.IsAny<UserExport>())).ReturnsAsync(true);
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observationManager = TestObject;

            var result = await observationManager.RunAsync(new SearchFilter(0), 0, "", null, "", ExportFormat.DwC, "en-GB", false, PropertyLabelType.PropertyName, false, false, false, null, false, null, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().Be(true);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task RunAsyncThrows()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _observationManager
                .Setup(blss => blss
                    .ExportAndSendAsync(
                        It.IsAny<int>(),
                        It.IsAny<string>(),
                        It.IsAny<SearchFilter>(),
                        It.IsAny<string>(),
                        "",
                        ExportFormat.DwC,
                        "en-GB",
                        false,
                        PropertyLabelType.PropertyName,
                        false,
                        It.IsAny<bool>(),
                        It.IsAny<bool>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        JobCancellationToken.Null)
                )
                .Throws(new Exception());
            _userExportRepository.Setup(uer => uer.GetAsync(It.IsAny<int>())).ReturnsAsync(new UserExport());
            _userExportRepository.Setup(uer => uer.AddOrUpdateAsync(It.IsAny<UserExport>())).ReturnsAsync(true);
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observationManager = TestObject;

            Func<Task> act = async () =>
            {
                await observationManager.RunAsync(new SearchFilter(0), 0, "", null, "", ExportFormat.DwC, "en-GB", false, PropertyLabelType.PropertyName, false, false, false, null, false, null, JobCancellationToken.Null);
            };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            await act.Should().ThrowAsync<Exception>();
        }
    }
}