using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.MongoDb.Interfaces;
using SOS.Import.Repositories.Destination.Nors;
using Xunit;

namespace SOS.Import.UnitTests.Repositories.Destination.Nors
{
    /// <summary>
    /// Meta data repository tests
    /// </summary>
    public class NorsObservationVerbatimRepositoryTests
    {
        private readonly Mock<IImportClient> _importClient;
        private readonly Mock<ILogger<NorsObservationVerbatimRepository>> _loggerMock;

        private NorsObservationVerbatimRepository TestObject => new NorsObservationVerbatimRepository(
            _importClient.Object,
            _loggerMock.Object);

        /// <summary>
        /// Constructor
        /// </summary>
        public NorsObservationVerbatimRepositoryTests()
        {
            _importClient = new Mock<IImportClient>();
            _loggerMock = new Mock<ILogger<NorsObservationVerbatimRepository>>();
        }

        /// <summary>
        /// Test the constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new NorsObservationVerbatimRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("importClient");

            create = () => new NorsObservationVerbatimRepository(
                _importClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

    }
}
