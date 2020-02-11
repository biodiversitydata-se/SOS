using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.Factories.Interfaces;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories;
using Xunit;

namespace SOS.Export.UnitTests.Repositories
{
    public class ProcessedSightingRepositoryTests
    {
        private readonly Mock<IExportClient> _exportClient;
        private readonly Mock<ITaxonFactory> _taxonFactory;
        private readonly Mock<ILogger<ProcessedSightingRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessedSightingRepositoryTests()
        {
            _exportClient = new Mock<IExportClient>();
            _taxonFactory = new Mock<ITaxonFactory>();
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
                _taxonFactory.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new ProcessedSightingRepository(
                null,
                _taxonFactory.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("exportClient");

            create = () => new ProcessedSightingRepository(
                _exportClient.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("taxonFactory");

            create = () => new ProcessedSightingRepository(
                _exportClient.Object,
                _taxonFactory.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}
