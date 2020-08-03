using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Repositories.Destination.DarwinCoreArchive;
using SOS.Lib.Database.Interfaces;
using Xunit;

namespace SOS.Import.UnitTests.Repositories.Destination.DarwinCoreArchive
{
    /// <summary>
    ///     Meta data repository tests
    /// </summary>
    public class DarwinCoreArchiveEventRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public DarwinCoreArchiveEventRepositoryTests()
        {
            _importClient = new Mock<IVerbatimClient>();
            _loggerMock = new Mock<ILogger<DarwinCoreArchiveEventRepository>>();
        }

        private readonly Mock<IVerbatimClient> _importClient;
        private readonly Mock<ILogger<DarwinCoreArchiveEventRepository>> _loggerMock;

        private DarwinCoreArchiveEventRepository TestObject => new DarwinCoreArchiveEventRepository(
            _importClient.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test the constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new DarwinCoreArchiveEventRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("importClient");

            create = () => new DarwinCoreArchiveEventRepository(
                _importClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}