using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.MongoDb.Interfaces;
using SOS.Import.Repositories.Destination.Artportalen;
using Xunit;

namespace SOS.Import.UnitTests.Repositories.Destination.Artportalen
{
    /// <summary>
    ///     Meta data repository tests
    /// </summary>
    public class SightingVerbatimRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public SightingVerbatimRepositoryTests()
        {
            _importClient = new Mock<IImportClient>();
            _loggerMock = new Mock<ILogger<SightingVerbatimRepository>>();
        }

        private readonly Mock<IImportClient> _importClient;
        private readonly Mock<ILogger<SightingVerbatimRepository>> _loggerMock;

        private SightingVerbatimRepository TestObject => new SightingVerbatimRepository(
            _importClient.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test the constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new SightingVerbatimRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("importClient");

            create = () => new SightingVerbatimRepository(
                _importClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}