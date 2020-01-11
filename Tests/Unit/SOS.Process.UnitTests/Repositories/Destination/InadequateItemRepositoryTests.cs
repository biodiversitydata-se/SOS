using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination;
using Xunit;

namespace SOS.Process.UnitTests.Repositories.Destination
{
    public class ProcessInfoRepositoryTests
    {
        private readonly Mock<IProcessClient> _processClient;
        private readonly Mock<ILogger<ProcessInfoRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessInfoRepositoryTests()
        {
            _processClient = new Mock<IProcessClient>();
            _loggerMock = new Mock<ILogger<ProcessInfoRepository>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            Action create = () => new ProcessInfoRepository(
                _processClient.Object,
                _loggerMock.Object);
            create.Should().Throw<NullReferenceException>();

           create = () => new ProcessInfoRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new ProcessInfoRepository(
                _processClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}
