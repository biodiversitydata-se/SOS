using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Resource;
using Xunit;

namespace SOS.Export.UnitTests.Repositories
{
    public class VocabularyRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public VocabularyRepositoryTests()
        {
            _exportClient = new Mock<IProcessClient>();
            _loggerMock = new Mock<ILogger<VocabularyRepository>>();
        }

        private readonly Mock<IProcessClient> _exportClient;
        private readonly Mock<ILogger<VocabularyRepository>> _loggerMock;

        private VocabularyRepository TestObject => new VocabularyRepository(
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

            Action create = () => new VocabularyRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("exportClient");

            create = () => new VocabularyRepository(
                _exportClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}