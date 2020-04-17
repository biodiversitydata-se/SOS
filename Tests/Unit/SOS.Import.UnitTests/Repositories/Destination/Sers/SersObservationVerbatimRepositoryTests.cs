using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.MongoDb.Interfaces;
using SOS.Import.Repositories.Destination.Sers;
using Xunit;

namespace SOS.Import.UnitTests.Repositories.Destination.Sers
{
    /// <summary>
    /// Meta data repository tests
    /// </summary>
    public class SersObservationVerbatimRepositoryTests
    {
        private readonly Mock<IImportClient> _importClient;
        private readonly Mock<ILogger<SersObservationVerbatimRepository>> _loggerMock;

        private SersObservationVerbatimRepository TestObject => new SersObservationVerbatimRepository(
            _importClient.Object,
            _loggerMock.Object);

        /// <summary>
        /// Constructor
        /// </summary>
        public SersObservationVerbatimRepositoryTests()
        {
            _importClient = new Mock<IImportClient>();
            _loggerMock = new Mock<ILogger<SersObservationVerbatimRepository>>();
        }

        /// <summary>
        /// Test the constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new SersObservationVerbatimRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("importClient");

            create = () => new SersObservationVerbatimRepository(
                _importClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

    }
}
