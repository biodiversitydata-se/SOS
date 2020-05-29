using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories;
using Xunit;

namespace SOS.Export.UnitTests.Repositories
{
    public class ProcessedTaxonRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ProcessedTaxonRepositoryTests()
        {
            _exportClient = new Mock<IExportClient>();
            _loggerMock = new Mock<ILogger<ProcessedTaxonRepository>>();
        }

        private readonly Mock<IExportClient> _exportClient;
        private readonly Mock<ILogger<ProcessedTaxonRepository>> _loggerMock;

        private ProcessedTaxonRepository TestObject => new ProcessedTaxonRepository(
            _exportClient.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new ProcessedTaxonRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("exportClient");

            create = () => new ProcessedTaxonRepository(
                _exportClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}