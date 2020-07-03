using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.Repositories;
using SOS.Lib.Database.Interfaces;
using Xunit;

namespace SOS.Export.UnitTests.Repositories
{
    public class ProcessInfoRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ProcessInfoRepositoryTests()
        {
            _exportClient = new Mock<IProcessClient>();
            _loggerMock = new Mock<ILogger<ProcessInfoRepository>>();
        }

        private readonly Mock<IProcessClient> _exportClient;
        private readonly Mock<ILogger<ProcessInfoRepository>> _loggerMock;

        private ProcessInfoRepository TestObject => new ProcessInfoRepository(
            _exportClient.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new ProcessInfoRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("exportClient");

            create = () => new ProcessInfoRepository(
                _exportClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}