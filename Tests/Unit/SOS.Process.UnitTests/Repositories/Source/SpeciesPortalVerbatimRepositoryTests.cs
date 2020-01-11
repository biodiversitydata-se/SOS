using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Source;
using Xunit;

namespace SOS.Process.UnitTests.Repositories.Source
{
    public class SpeciesPortalVerbatimRepositoryTests
    {
        private readonly Mock<IVerbatimClient> _processClient;
        private readonly Mock<ILogger<SpeciesPortalVerbatimRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public SpeciesPortalVerbatimRepositoryTests()
        {
            _processClient = new Mock<IVerbatimClient>();
            _loggerMock = new Mock<ILogger<SpeciesPortalVerbatimRepository>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new SpeciesPortalVerbatimRepository(
                _processClient.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new SpeciesPortalVerbatimRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new SpeciesPortalVerbatimRepository(
                _processClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}
