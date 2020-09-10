using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Lib.Database.Interfaces;
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
            _processClient = new Mock<IProcessClient>();
            _loggerMock = new Mock<ILogger<FieldMappingRepository>>();
        }

        private readonly Mock<IProcessClient> _processClient;
        private readonly Mock<ILogger<FieldMappingRepository>> _loggerMock;

        private FieldMappingRepository TestObject => new FieldMappingRepository(
            _processClient.Object,
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
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processClient");

            create = () => new FieldMappingRepository(
                _processClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}