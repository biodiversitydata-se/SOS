using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;
using SOS.Lib.Configuration.Shared;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Destination.Interfaces;
using Xunit;

namespace SOS.Process.UnitTests.Repositories.Destination
{
    public class ProcessedObservationRepositoryTests
    {
        private readonly Mock<IProcessClient> _processClient;
        private readonly Mock<IElasticClient> _elasticClient;
        private readonly Mock<IInvalidObservationRepository> _invalidObservationRepositoryMock;
        private readonly ElasticSearchConfiguration _elasticSearchConfiguration;
        private readonly Mock<ILogger<ProcessedObservationRepository>> _loggerMock;
       

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessedObservationRepositoryTests()
        {
            _processClient = new Mock<IProcessClient>();
            _elasticClient = new Mock<IElasticClient>();
            _invalidObservationRepositoryMock = new Mock<IInvalidObservationRepository>();
            _elasticSearchConfiguration = new ElasticSearchConfiguration();
            _loggerMock = new Mock<ILogger<ProcessedObservationRepository>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            Action create = () => new ProcessedObservationRepository(
                null,
                _elasticClient.Object,
                _invalidObservationRepositoryMock.Object,
                _elasticSearchConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new ProcessedObservationRepository(
                _processClient.Object,
                null,
                _invalidObservationRepositoryMock.Object,
                _elasticSearchConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("elasticClient");

            create = () => new ProcessedObservationRepository(
                _processClient.Object,
                _elasticClient.Object,
                null,
                _elasticSearchConfiguration,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("invalidObservationRepository");

            create = () => new ProcessedObservationRepository(
                _processClient.Object,
                _elasticClient.Object,
                _invalidObservationRepositoryMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("elasticConfiguration");

            create = () => new ProcessedObservationRepository(
                _processClient.Object,
                _elasticClient.Object,
                _invalidObservationRepositoryMock.Object,
                _elasticSearchConfiguration,
            null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}
