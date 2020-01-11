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
    public class DarwinCoreRepositoryTests
    {
        private readonly Mock<IProcessClient> _processClient;
        private readonly Mock<IInadequateItemRepository> _inadequateItemRepositoryMock;
        private readonly Mock<ILogger<DarwinCoreRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public DarwinCoreRepositoryTests()
        {
            _processClient = new Mock<IProcessClient>();
            _inadequateItemRepositoryMock = new Mock<IInadequateItemRepository>();
            _loggerMock = new Mock<ILogger<DarwinCoreRepository>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new DarwinCoreRepository(
                _processClient.Object,
                _inadequateItemRepositoryMock.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new DarwinCoreRepository(
                null,
                _inadequateItemRepositoryMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new DarwinCoreRepository(
                _processClient.Object,
               null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("darwinCoreInadequateRepository");

            create = () => new DarwinCoreRepository(
                _processClient.Object,
                _inadequateItemRepositoryMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}
