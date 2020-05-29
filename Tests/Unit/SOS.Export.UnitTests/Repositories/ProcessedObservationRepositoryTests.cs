using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories;
using SOS.Lib.Configuration.Shared;
using Xunit;

namespace SOS.Export.UnitTests.Repositories
{
    public class ProcessedObservationRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ProcessedObservationRepositoryTests()
        {
            _elasticClient = new Mock<IElasticClient>();
            _exportClient = new Mock<IExportClient>();
            _elasticSearchConfiguration = new ElasticSearchConfiguration();
            _loggerMock = new Mock<ILogger<ProcessedObservationRepository>>();
        }

        private readonly Mock<IElasticClient> _elasticClient;
        private readonly Mock<IExportClient> _exportClient;
        private readonly ElasticSearchConfiguration _elasticSearchConfiguration;
        private readonly Mock<ILogger<ProcessedObservationRepository>> _loggerMock;

        private ProcessedObservationRepository TestObject => new ProcessedObservationRepository(
            _elasticClient.Object,
            _exportClient.Object,
            _elasticSearchConfiguration,
            _loggerMock.Object);

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new ProcessedObservationRepository(
                null,
                _exportClient.Object,
                _elasticSearchConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("elasticClient");

            create = () => new ProcessedObservationRepository(
                _elasticClient.Object,
                null,
                _elasticSearchConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("exportClient");

            create = () => new ProcessedObservationRepository(
                _elasticClient.Object,
                _exportClient.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("elasticConfiguration");

            create = () => new ProcessedObservationRepository(
                _elasticClient.Object,
                _exportClient.Object,
                _elasticSearchConfiguration,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}