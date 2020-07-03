using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Database.Interfaces;
using SOS.Process.Repositories.Source;
using Xunit;

namespace SOS.Process.UnitTests.Repositories.Source
{
    public class AreaVerbatimRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public AreaVerbatimRepositoryTests()
        {
            _processClient = new Mock<IVerbatimClient>();
            _loggerMock = new Mock<ILogger<AreaVerbatimRepository>>();
        }

        private readonly Mock<IVerbatimClient> _processClient;
        private readonly Mock<ILogger<AreaVerbatimRepository>> _loggerMock;

        private AreaVerbatimRepository TestObject => new AreaVerbatimRepository(
            _processClient.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new AreaVerbatimRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new AreaVerbatimRepository(
                _processClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}