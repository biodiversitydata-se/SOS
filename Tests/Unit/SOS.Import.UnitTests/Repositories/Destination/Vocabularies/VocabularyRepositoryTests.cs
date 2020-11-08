using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Resource;
using Xunit;

namespace SOS.Import.UnitTests.Repositories.Destination.Vocabularies
{
    /// <summary>
    ///     Meta data repository tests
    /// </summary>
    public class VocabularyRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public VocabularyRepositoryTests()
        {
            _processClient = new Mock<IProcessClient>();
            _loggerMock = new Mock<ILogger<VocabularyRepository>>();
        }

        private readonly Mock<IProcessClient> _processClient;
        private readonly Mock<ILogger<VocabularyRepository>> _loggerMock;

        private VocabularyRepository TestObject => new VocabularyRepository(
            _processClient.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test the constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new VocabularyRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processClient");

            create = () => new VocabularyRepository(
                _processClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}