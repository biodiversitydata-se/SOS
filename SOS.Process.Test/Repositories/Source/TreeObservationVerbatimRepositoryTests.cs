using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Source;
using Xunit;

namespace SOS.Process.Test.Repositories.Source
{
    public class TreeObservationVerbatimRepositoryTests
    {
        private readonly Mock<IVerbatimClient> _processClient;
        private readonly Mock<ILogger<TreeObservationVerbatimRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public TreeObservationVerbatimRepositoryTests()
        {
            _processClient = new Mock<IVerbatimClient>();
            _loggerMock = new Mock<ILogger<TreeObservationVerbatimRepository>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new TreeObservationVerbatimRepository(
                _processClient.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new TreeObservationVerbatimRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new TreeObservationVerbatimRepository(
                _processClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}
