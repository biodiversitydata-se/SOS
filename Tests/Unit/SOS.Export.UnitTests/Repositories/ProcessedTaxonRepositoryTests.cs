using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Resource;
using Xunit;

namespace SOS.Export.UnitTests.Repositories
{
    public class ProcessedTaxonRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ProcessedTaxonRepositoryTests()
        {
            _exportClient = new Mock<IProcessClient>();
            _loggerMock = new Mock<ILogger<TaxonRepository>>();
        }

        private readonly Mock<IProcessClient> _exportClient;
        private readonly Mock<ILogger<TaxonRepository>> _loggerMock;

        private TaxonRepository TestObject => new TaxonRepository(
            _exportClient.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new TaxonRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("exportClient");

            create = () => new TaxonRepository(
                _exportClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}