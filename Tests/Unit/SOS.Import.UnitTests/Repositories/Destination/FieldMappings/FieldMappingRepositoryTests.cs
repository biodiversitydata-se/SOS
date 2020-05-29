using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.MongoDb.Interfaces;
using SOS.Import.Repositories.Destination.FieldMappings;
using Xunit;

namespace SOS.Import.UnitTests.Repositories.Destination.FieldMappings
{
    /// <summary>
    ///     Meta data repository tests
    /// </summary>
    public class FieldMappingRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public FieldMappingRepositoryTests()
        {
            _importClient = new Mock<IImportClient>();
            _loggerMock = new Mock<ILogger<FieldMappingRepository>>();
        }

        private readonly Mock<IImportClient> _importClient;
        private readonly Mock<ILogger<FieldMappingRepository>> _loggerMock;

        private FieldMappingRepository TestObject => new FieldMappingRepository(
            _importClient.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test the constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new FieldMappingRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("importClient");

            create = () => new FieldMappingRepository(
                _importClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}