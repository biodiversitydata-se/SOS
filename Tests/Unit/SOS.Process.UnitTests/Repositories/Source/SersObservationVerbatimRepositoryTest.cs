using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using Xunit;

namespace SOS.Process.UnitTests.Repositories.Source
{
    public class SersObservationVerbatimRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public SersObservationVerbatimRepositoryTests()
        {
            _processClient = new Mock<IVerbatimClient>();
            _loggerMock = new Mock<ILogger<SersObservationVerbatimRepository>>();
        }

        private readonly Mock<IVerbatimClient> _processClient;
        private readonly Mock<ILogger<SersObservationVerbatimRepository>> _loggerMock;

        private SersObservationVerbatimRepository TestObject => new SersObservationVerbatimRepository(
            _processClient.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test constructor
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