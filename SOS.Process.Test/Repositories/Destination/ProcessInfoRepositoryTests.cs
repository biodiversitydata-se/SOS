using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination;
using Xunit;

namespace SOS.Process.Test.Repositories.Destination
{
    public class InadequateItemRepositoryTests
    {
        private readonly Mock<IProcessClient> _processClient;
        private readonly Mock<ILogger<InadequateItemRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public InadequateItemRepositoryTests()
        {
            _processClient = new Mock<IProcessClient>();
            _loggerMock = new Mock<ILogger<InadequateItemRepository>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new InadequateItemRepository(
                _processClient.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new InadequateItemRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new InadequateItemRepository(
                _processClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}
