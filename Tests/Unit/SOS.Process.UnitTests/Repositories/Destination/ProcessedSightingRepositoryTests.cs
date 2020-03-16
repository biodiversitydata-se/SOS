using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Destination.Interfaces;
using Xunit;

namespace SOS.Process.UnitTests.Repositories.Destination
{
    public class ProcessedSightingRepositoryTests
    {
        private readonly Mock<IProcessClient> _processClient;
        private readonly Mock<IInvalidObservationRepository> _invalidObservationRepositoryMock;
        private readonly Mock<ILogger<ProcessedSightingRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessedSightingRepositoryTests()
        {
            _processClient = new Mock<IProcessClient>();
            _invalidObservationRepositoryMock = new Mock<IInvalidObservationRepository>();
            _loggerMock = new Mock<ILogger<ProcessedSightingRepository>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new ProcessedSightingRepository(
                _processClient.Object,
                _invalidObservationRepositoryMock.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new ProcessedSightingRepository(
                null,
                _invalidObservationRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new ProcessedSightingRepository(
                _processClient.Object,
               null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("invalidObservationRepository");

            create = () => new ProcessedSightingRepository(
                _processClient.Object,
                _invalidObservationRepositoryMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}
