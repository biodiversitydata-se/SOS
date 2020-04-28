using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.MongoDb.Interfaces;
using SOS.Import.Repositories.Destination.Shark;
using Xunit;

namespace SOS.Import.UnitTests.Repositories.Destination.Shark
{
    /// <summary>
    /// Meta data repository tests
    /// </summary>
    public class SharkObservationVerbatimRepositoryTests
    {
        private readonly Mock<IImportClient> _importClient;
        private readonly Mock<ILogger<SharkObservationVerbatimRepository>> _loggerMock;

        private SharkObservationVerbatimRepository TestObject => new SharkObservationVerbatimRepository(
            _importClient.Object,
            _loggerMock.Object);

        /// <summary>
        /// Constructor
        /// </summary>
        public SharkObservationVerbatimRepositoryTests()
        {
            _importClient = new Mock<IImportClient>();
            _loggerMock = new Mock<ILogger<SharkObservationVerbatimRepository>>();
        }

        /// <summary>
        /// Test the constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new SharkObservationVerbatimRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("importClient");

            create = () => new SharkObservationVerbatimRepository(
                _importClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

    }
}
