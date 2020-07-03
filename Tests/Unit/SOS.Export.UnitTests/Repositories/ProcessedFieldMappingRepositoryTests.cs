using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.Repositories;
using SOS.Lib.Database.Interfaces;
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
            _loggerMock = new Mock<ILogger<ProcessedFieldMappingRepository>>();
        }

        private readonly Mock<IProcessClient> _exportClient;
        private readonly Mock<ILogger<ProcessedFieldMappingRepository>> _loggerMock;

        private ProcessedFieldMappingRepository TestObject => new ProcessedFieldMappingRepository(
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

            Action create = () => new ProcessedFieldMappingRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("exportClient");

            create = () => new ProcessedFieldMappingRepository(
                _exportClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}