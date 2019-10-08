using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination;
using Xunit;

namespace SOS.Process.Test.Repositories.Destination
{
    public class ProcessedRepositoryTests
    {
        private readonly Mock<IProcessClient> _processClient;
        private readonly Mock<ILogger<ProcessedRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessedRepositoryTests()
        {
            _processClient = new Mock<IProcessClient>();
            _loggerMock = new Mock<ILogger<ProcessedRepository>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new ProcessedRepository(
                _processClient.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new ProcessedRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new ProcessedRepository(
                _processClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}
