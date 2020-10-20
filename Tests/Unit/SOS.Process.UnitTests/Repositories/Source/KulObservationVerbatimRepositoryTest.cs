using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using Xunit;

namespace SOS.Process.UnitTests.Repositories.Source
{
    public class KulObservationVerbatimRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public KulObservationVerbatimRepositoryTests()
        {
            _processClient = new Mock<IVerbatimClient>();
            _loggerMock = new Mock<ILogger<KulObservationVerbatimRepository>>();
        }

        private readonly Mock<IVerbatimClient> _processClient;
        private readonly Mock<ILogger<KulObservationVerbatimRepository>> _loggerMock;

        private KulObservationVerbatimRepository TestObject => new KulObservationVerbatimRepository(
            _processClient.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new KulObservationVerbatimRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new KulObservationVerbatimRepository(
                _processClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}