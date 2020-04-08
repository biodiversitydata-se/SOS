using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.Managers;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using Xunit;

namespace SOS.Export.UnitTests.Managers
{
    /// <summary>
    /// Tests for observation manager
    /// </summary>
    public class TaxonManagerTests
    {
        private readonly Mock<IProcessedTaxonRepository> _processedTaxonRepositoryMock;
        private readonly Mock<ILogger<TaxonManager>> _loggerMock;

        /// <summary>
        /// Return object to be tested
        /// </summary>
        private TaxonManager TestObject => new TaxonManager(
            _processedTaxonRepositoryMock.Object,
            _loggerMock.Object);

        /// <summary>
        /// Constructor
        /// </summary>
        public TaxonManagerTests()
        {
            _processedTaxonRepositoryMock = new Mock<IProcessedTaxonRepository>();
            _loggerMock = new Mock<ILogger<TaxonManager>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new TaxonManager(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("processedTaxonRepository");

            create = () => new TaxonManager(
                _processedTaxonRepositoryMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Make a successful taxon tree
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task TaxonTreeSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processedTaxonRepositoryMock.Setup(pir => pir.GetBasicTaxonChunkAsync(It.Is<int>(i => i == 0), It.IsAny<int>()))
                .ReturnsAsync(new[] { new ProcessedBasicTaxon() });
            _processedTaxonRepositoryMock.Setup(pir => pir.GetBasicTaxonChunkAsync(It.Is<int>(i => i != 0), It.IsAny<int>()))
                .ReturnsAsync(new ProcessedBasicTaxon[0]);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var taxonManager = TestObject;

            var tree = taxonManager.TaxonTree;
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            tree.Root.TaxonId.Should().Be(0);
        }

        /// <summary>
        /// Test taxon tree fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task TaxonTreeFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processedTaxonRepositoryMock.Setup(pir => pir.GetBasicTaxonChunkAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ProcessedBasicTaxon[0]);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var taxonManager = TestObject;

            var tree = taxonManager.TaxonTree;
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            tree.Should().BeNull();
        }

        /// <summary>
        /// Test taxon tree throws
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task TaxonTreeThrows()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processedTaxonRepositoryMock.Setup(pir => pir.GetBasicTaxonChunkAsync(It.Is<int>(i => i == 0), It.IsAny<int>()))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var taxonManager = TestObject;

            var tree = taxonManager.TaxonTree;
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            tree.Should().BeNull();
        }
    }
}
