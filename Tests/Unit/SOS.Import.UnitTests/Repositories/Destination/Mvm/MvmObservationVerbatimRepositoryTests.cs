using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.MongoDb.Interfaces;
using SOS.Import.Repositories.Destination.Mvm;
using Xunit;

namespace SOS.Import.UnitTests.Repositories.Destination.Mvm
{
    /// <summary>
    /// Meta data repository tests
    /// </summary>
    public class MvmObservationVerbatimRepositoryTests
    {
        private readonly Mock<IImportClient> _importClient;
        private readonly Mock<ILogger<MvmObservationVerbatimRepository>> _loggerMock;

        private MvmObservationVerbatimRepository TestObject => new MvmObservationVerbatimRepository(
            _importClient.Object,
            _loggerMock.Object);

        /// <summary>
        /// Constructor
        /// </summary>
        public MvmObservationVerbatimRepositoryTests()
        {
            _importClient = new Mock<IImportClient>();
            _loggerMock = new Mock<ILogger<MvmObservationVerbatimRepository>>();
        }

        /// <summary>
        /// Test the constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new MvmObservationVerbatimRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("importClient");

            create = () => new MvmObservationVerbatimRepository(
                _importClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

    }
}
