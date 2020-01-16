using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories;
using Xunit;

namespace SOS.Export.UnitTests.Repositories
{
    public class ProcessedSightingRepositoryTests
    {
        private readonly Mock<IExportClient> _exportClient;
        private readonly Mock<ILogger<ProcessedSightingRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessedSightingRepositoryTests()
        {
            _exportClient = new Mock<IExportClient>();
            _loggerMock = new Mock<ILogger<ProcessedSightingRepository>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        [Trait("Category","Unit")]
        public void ConstructorTest()
        {
            new ProcessedSightingRepository(
                _exportClient.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new ProcessedSightingRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("exportClient");

            create = () => new ProcessedSightingRepository(
                _exportClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}
