using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Source;
using Xunit;

namespace SOS.Process.UnitTests.Repositories.Source
{
    public class SersObservationVerbatimRepositoryTests
    {
        private readonly Mock<IVerbatimClient> _processClient;
        private readonly Mock<ILogger<SersObservationVerbatimRepository>> _loggerMock;

        private SersObservationVerbatimRepository TestObject => new SersObservationVerbatimRepository(
            _processClient.Object,
            _loggerMock.Object);

        /// <summary>
        /// Constructor
        /// </summary>
        public SersObservationVerbatimRepositoryTests()
        {
            _processClient = new Mock<IVerbatimClient>();
            _loggerMock = new Mock<ILogger<SersObservationVerbatimRepository>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new SersObservationVerbatimRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new SersObservationVerbatimRepository(
                _processClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}
