using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;
using SOS.Export.Managers.Interfaces;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories;
using Xunit;

namespace SOS.Export.UnitTests.Repositories
{
    public class ProcessedObservationRepositoryTests
    {
        
        private readonly Mock<IElasticClient> _elasticClient;
        private readonly Mock<IExportClient> _exportClient;
        private readonly Mock<ITaxonManager> _taxonManager;
        private readonly Mock<ILogger<ProcessedObservationRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessedObservationRepositoryTests()
        {
            _elasticClient = new Mock<IElasticClient>();
            _exportClient = new Mock<IExportClient>();
            _taxonManager = new Mock<ITaxonManager>();
            _loggerMock = new Mock<ILogger<ProcessedObservationRepository>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        [Trait("Category","Unit")]
        public void ConstructorTest()
        {
            new ProcessedObservationRepository(
                _elasticClient.Object,
                _exportClient.Object,
                _taxonManager.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new ProcessedObservationRepository(
                null,
                _exportClient.Object,
                _taxonManager.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("elasticClient");

            create = () => new ProcessedObservationRepository(
                _elasticClient.Object,
                null,
                _taxonManager.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("exportClient");

            create = () => new ProcessedObservationRepository(
                _elasticClient.Object,
                _exportClient.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("taxonFactory");

            create = () => new ProcessedObservationRepository(
                _elasticClient.Object,
                _exportClient.Object,
                _taxonManager.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}
