using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Process.Factories;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;
using Xunit;

namespace SOS.Process.Test.Factories
{
    /// <summary>
    /// Tests for sighting factory
    /// </summary>
    public class ClamPortalProcessFactoryTests
    {
        private readonly Mock<IClamObservationVerbatimRepository> _clamObservationVerbatimRepositoryMock;
        private readonly Mock<IAreaHelper> _areaHelper;
        private readonly Mock<IDarwinCoreRepository> _DarwinCoreRepository;
        private readonly Mock<ILogger<ClamPortalProcessFactory>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ClamPortalProcessFactoryTests()
        {
            _clamObservationVerbatimRepositoryMock = new Mock<IClamObservationVerbatimRepository>();
            _areaHelper = new Mock<IAreaHelper>();
            _DarwinCoreRepository = new Mock<IDarwinCoreRepository>();
            _loggerMock = new Mock<ILogger<ClamPortalProcessFactory>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new ClamPortalProcessFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _areaHelper.Object,
                _DarwinCoreRepository.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () =>  new ClamPortalProcessFactory(
                null,
                _areaHelper.Object,
                _DarwinCoreRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("clamObservationVerbatimRepository");

            
            create = () => new ClamPortalProcessFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                null,
                _DarwinCoreRepository.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("areaHelper");

            create = () => new ClamPortalProcessFactory(
                 _clamObservationVerbatimRepositoryMock.Object,
                 _areaHelper.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("DarwinCoreRepository");

            create = () => new ClamPortalProcessFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _areaHelper.Object,
                _DarwinCoreRepository.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Make a successful test of processing
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ProcessAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _clamObservationVerbatimRepositoryMock.Setup(r => r.GetBatchAsync(0))
                .ReturnsAsync(new [] { new ClamObservationVerbatim 
                {
                    DyntaxaTaxonId = 0
                } });

            _areaHelper.Setup(r => r.AddAreaDataToDarwinCore(It.IsAny<IEnumerable<DarwinCore<DynamicProperties>>>()));

            _DarwinCoreRepository.Setup(r => r.AddManyAsync(It.IsAny<ICollection<DarwinCore<DynamicProperties>>>()))
                .ReturnsAsync(true);

            var taxa = new Dictionary<int, DarwinCoreTaxon>
            {
                { 0, new DarwinCoreTaxon { TaxonID = "0", ScientificName = "Biota" } }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var clamPortalProcessFactory = new ClamPortalProcessFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _areaHelper.Object,
                _DarwinCoreRepository.Object,
                _loggerMock.Object);

            var result = await clamPortalProcessFactory.ProcessAsync( taxa, JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }

        /// <summary>
        /// Test processing fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AggregateAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var clamPortalProcessFactory = new ClamPortalProcessFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _areaHelper.Object,
                _DarwinCoreRepository.Object,
                _loggerMock.Object);

            var result = await clamPortalProcessFactory.ProcessAsync(null, JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }

        /// <summary>
        /// Test processing exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ProcessAsyncException()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _clamObservationVerbatimRepositoryMock.Setup(r => r.GetBatchAsync(0))
                .ThrowsAsync(new Exception("Failed"));
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var clamPortalProcessFactory = new ClamPortalProcessFactory(
                _clamObservationVerbatimRepositoryMock.Object,
                _areaHelper.Object,
                _DarwinCoreRepository.Object,
                _loggerMock.Object);

            var result = await clamPortalProcessFactory.ProcessAsync( null, JobCancellationToken.Null);
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }
    }
}
