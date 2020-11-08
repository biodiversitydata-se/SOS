using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Resource;
using Xunit;

namespace SOS.Process.UnitTests.Repositories.Destination
{
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

        /// <summary>
        ///     Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            Action create = () => new VocabularyRepository(
                _processClient.Object,
                _loggerMock.Object);
            create.Should().Throw<NullReferenceException>();

            create = () => new VocabularyRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new VocabularyRepository(
                _processClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}