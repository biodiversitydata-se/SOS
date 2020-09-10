using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Repositories.Destination.Area;
using SOS.Lib.Database.Interfaces;
using Xunit;

namespace SOS.Import.UnitTests.Repositories.Destination.Artportalen
{
    /// <summary>
    ///     Meta data repository tests
    /// </summary>
    public class AreaProcessedRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public AreaProcessedRepositoryTests()
        {
            _importClient = new Mock<IProcessClient>();
            _loggerMock = new Mock<ILogger<AreaProcessedRepository>>();
        }

        private readonly Mock<IProcessClient> _importClient;
        private readonly Mock<ILogger<AreaProcessedRepository>> _loggerMock;

        private AreaProcessedRepository TestObject => new AreaProcessedRepository(
            _importClient.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test the constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new AreaProcessedRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new AreaProcessedRepository(
                _importClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}