using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Database.Interfaces;
using SOS.Import.Repositories.Destination.VirtualHerbarium;
using Xunit;

namespace SOS.Import.UnitTests.Repositories.Destination.VirtualHerbarium
{
    /// <summary>
    ///     Meta data repository tests
    /// </summary>
    public class VirtualHerbariumObservationVerbatimRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public VirtualHerbariumObservationVerbatimRepositoryTests()
        {
            _importClient = new Mock<IVerbatimClient>();
            _loggerMock = new Mock<ILogger<VirtualHerbariumObservationVerbatimRepository>>();
        }

        private readonly Mock<IVerbatimClient> _importClient;
        private readonly Mock<ILogger<VirtualHerbariumObservationVerbatimRepository>> _loggerMock;

        private VirtualHerbariumObservationVerbatimRepository TestObject =>
            new VirtualHerbariumObservationVerbatimRepository(
                _importClient.Object,
                _loggerMock.Object);

        /// <summary>
        ///     Test the constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new VirtualHerbariumObservationVerbatimRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("importClient");

            create = () => new VirtualHerbariumObservationVerbatimRepository(
                _importClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}