using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Destination.Interfaces;
using Xunit;

namespace SOS.Process.UnitTests.Repositories.Destination
{
    public class ProcessedObservationRepositoryTests
    {
        private readonly Mock<IProcessClient> _processClient;
        private readonly Mock<IInvalidObservationRepository> _invalidObservationRepositoryMock;
        private readonly Mock<ILogger<ProcessedObservationRepository>> _loggerMock;
        private readonly Mock<IElasticClient> _elasticClient;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessedObservationRepositoryTests()
        {
            _processClient = new Mock<IProcessClient>();
            _invalidObservationRepositoryMock = new Mock<IInvalidObservationRepository>();
            _loggerMock = new Mock<ILogger<ProcessedObservationRepository>>();
            _elasticClient = new Mock<IElasticClient>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            Action create = () => new ProcessedObservationRepository(
                null,
                _invalidObservationRepositoryMock.Object,
                _loggerMock.Object,
                _elasticClient.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new ProcessedObservationRepository(
                _processClient.Object,
                _invalidObservationRepositoryMock.Object,
                null,
                _elasticClient.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}
