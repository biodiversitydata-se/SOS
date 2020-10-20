using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Processed;
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
            _exportClient = new Mock<IProcessClient>();
            _elasticSearchConfiguration = new ElasticSearchConfiguration();
            _loggerMock = new Mock<ILogger<ProcessedObservationRepository>>();
        }

        private readonly Mock<IElasticClient> _elasticClient;
        private readonly Mock<IProcessClient> _exportClient;
        private readonly ElasticSearchConfiguration _elasticSearchConfiguration;
        private readonly Mock<ILogger<ProcessedObservationRepository>> _loggerMock;

        private ProcessedObservationRepository TestObject => new ProcessedObservationRepository(
            _exportClient.Object,
            _elasticClient.Object,
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
                _elasticClient.Object,
                _elasticSearchConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new ProcessedObservationRepository(
                _exportClient.Object,
                null,
                _elasticSearchConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("elasticClient");

            create = () => new ProcessedObservationRepository(
                _exportClient.Object,
                _elasticClient.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("elasticConfiguration");

            create = () => new ProcessedObservationRepository(
                _exportClient.Object,
                _elasticClient.Object,
                _elasticSearchConfiguration,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}