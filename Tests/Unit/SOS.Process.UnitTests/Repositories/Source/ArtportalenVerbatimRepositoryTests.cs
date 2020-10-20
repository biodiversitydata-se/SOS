using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using Xunit;

namespace SOS.Process.UnitTests.Repositories.Source
{
    public class ArtportalenVerbatimRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ArtportalenVerbatimRepositoryTests()
        {
            _processClient = new Mock<IVerbatimClient>();
            _loggerMock = new Mock<ILogger<ArtportalenVerbatimRepository>>();
        }

        private readonly Mock<IVerbatimClient> _processClient;
        private readonly Mock<ILogger<ArtportalenVerbatimRepository>> _loggerMock;

        private ArtportalenVerbatimRepository TestObject => new ArtportalenVerbatimRepository(
            _processClient.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new ArtportalenVerbatimRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new ArtportalenVerbatimRepository(
                _processClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}