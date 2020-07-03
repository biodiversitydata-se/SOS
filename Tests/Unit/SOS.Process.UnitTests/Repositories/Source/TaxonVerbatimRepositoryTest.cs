using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Database.Interfaces;
using SOS.Process.Repositories.Source;
using Xunit;

namespace SOS.Process.UnitTests.Repositories.Source
{
    public class TaxonVerbatimRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public TaxonVerbatimRepositoryTests()
        {
            _processClient = new Mock<IVerbatimClient>();
            _loggerMock = new Mock<ILogger<TaxonVerbatimRepository>>();
        }

        private readonly Mock<IVerbatimClient> _processClient;
        private readonly Mock<ILogger<TaxonVerbatimRepository>> _loggerMock;

        private TaxonVerbatimRepository TestObject => new TaxonVerbatimRepository(
            _processClient.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new TaxonVerbatimRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new TaxonVerbatimRepository(
                _processClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}