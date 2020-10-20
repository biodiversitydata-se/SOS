using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using Xunit;

namespace SOS.Import.UnitTests.Repositories.Destination.Sers
{
    /// <summary>
    ///     Meta data repository tests
    /// </summary>
    public class SersObservationVerbatimRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public SersObservationVerbatimRepositoryTests()
        {
            _importClient = new Mock<IVerbatimClient>();
            _loggerMock = new Mock<ILogger<SersObservationVerbatimRepository>>();
        }

        private readonly Mock<IVerbatimClient> _importClient;
        private readonly Mock<ILogger<SersObservationVerbatimRepository>> _loggerMock;

        private SersObservationVerbatimRepository TestObject => new SersObservationVerbatimRepository(
            _importClient.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test the constructor
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