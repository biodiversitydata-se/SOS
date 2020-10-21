using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Repositories.Resource;
using Xunit;

namespace SOS.Export.UnitTests.Repositories
{
    public class ProcessedFieldMappingRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ProcessedFieldMappingRepositoryTests()
        {
            _exportClient = new Mock<IProcessClient>();
            _loggerMock = new Mock<ILogger<FieldMappingRepository>>();
        }

        private readonly Mock<IProcessClient> _exportClient;
        private readonly Mock<ILogger<FieldMappingRepository>> _loggerMock;

        private FieldMappingRepository TestObject => new FieldMappingRepository(
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

            Action create = () => new FieldMappingRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("exportClient");

            create = () => new FieldMappingRepository(
                _exportClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}