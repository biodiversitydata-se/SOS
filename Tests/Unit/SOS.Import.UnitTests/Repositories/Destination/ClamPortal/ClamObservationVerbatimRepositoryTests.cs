using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.MongoDb.Interfaces;
using SOS.Import.Repositories.Destination.ClamPortal;
using Xunit;

namespace SOS.Import.UnitTests.Repositories.Destination.ClamPortal
{
    /// <summary>
    ///     Meta data repository tests
    /// </summary>
    public class ClamObservationVerbatimRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ClamObservationVerbatimRepositoryTests()
        {
            _importClient = new Mock<IImportClient>();
            _loggerMock = new Mock<ILogger<ClamObservationVerbatimRepository>>();
        }

        private readonly Mock<IImportClient> _importClient;
        private readonly Mock<ILogger<ClamObservationVerbatimRepository>> _loggerMock;

        private ClamObservationVerbatimRepository TestObject => new ClamObservationVerbatimRepository(
            _importClient.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test the constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new ClamObservationVerbatimRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("importClient");

            create = () => new ClamObservationVerbatimRepository(
                _importClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}