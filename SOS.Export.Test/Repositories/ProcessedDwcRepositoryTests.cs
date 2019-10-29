using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories;
using Xunit;

namespace SOS.Export.Test.Repositories.Destination
{
    public class ProcessedDwcRepositoryTests
    {
        private readonly Mock<IExportClient> _exportClient;
        private readonly Mock<ILogger<ProcessedDarwinCoreRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessedDwcRepositoryTests()
        {
            _exportClient = new Mock<IExportClient>();
            _loggerMock = new Mock<ILogger<ProcessedDarwinCoreRepository>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new ProcessedDarwinCoreRepository(
                _exportClient.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new ProcessedDarwinCoreRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("exportClient");

            create = () => new ProcessedDarwinCoreRepository(
                _exportClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}
