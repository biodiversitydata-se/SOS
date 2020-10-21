using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Resource;
using Xunit;

namespace SOS.Process.UnitTests.Repositories.Destination
{
    public class ProcessedAreaRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ProcessedAreaRepositoryTests()
        {
            _processClient = new Mock<IProcessClient>();
            _loggerMock = new Mock<ILogger<AreaRepository>>();
        }

        private readonly Mock<IProcessClient> _processClient;
        private readonly Mock<ILogger<AreaRepository>> _loggerMock;

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            Action create = () => new AreaRepository(
                _processClient.Object,
                _loggerMock.Object);
            create.Should().Throw<NullReferenceException>();

            create = () => new AreaRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new AreaRepository(
                _processClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}