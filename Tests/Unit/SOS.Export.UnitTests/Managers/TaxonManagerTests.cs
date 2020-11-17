using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Export.Managers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Resource.Interfaces;
using Xunit;

namespace SOS.Export.UnitTests.Managers
{
    /// <summary>
    ///     Tests for observation manager
    /// </summary>
    public class TaxonManagerTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public TaxonManagerTests()
        {
            _processedTaxonRepositoryMock = new Mock<ITaxonRepository>();
            _loggerMock = new Mock<ILogger<TaxonManager>>();
        }

        private readonly Mock<ITaxonRepository> _processedTaxonRepositoryMock;
        private readonly Mock<ILogger<TaxonManager>> _loggerMock;

        /// <summary>
        ///     Return object to be tested
        /// </summary>
        private TaxonManager TestObject => new TaxonManager(
            _processedTaxonRepositoryMock.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test taxon tree fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Unit")]
        public void TaxonTreeFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processedTaxonRepositoryMock.Setup(pir => pir.GetBasicTaxonChunkAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new BasicTaxon[0]);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var tree = TestObject.TaxonTree;
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            tree.Should().BeNull();
        }

        /// <summary>
        ///     Make a successful taxon tree
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Unit")]
        public void TaxonTreeSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processedTaxonRepositoryMock
                .Setup(pir => pir.GetBasicTaxonChunkAsync(It.Is<int>(i => i == 0), It.IsAny<int>()))
                .ReturnsAsync(new[] {new BasicTaxon()});
            _processedTaxonRepositoryMock
                .Setup(pir => pir.GetBasicTaxonChunkAsync(It.Is<int>(i => i != 0), It.IsAny<int>()))
                .ReturnsAsync(new BasicTaxon[0]);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var tree = TestObject.TaxonTree;
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            tree.Root.TaxonId.Should().Be(0);
        }

        /// <summary>
        ///     Test taxon tree throws
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Unit")]
        public void TaxonTreeThrows()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processedTaxonRepositoryMock
                .Setup(pir => pir.GetBasicTaxonChunkAsync(It.Is<int>(i => i == 0), It.IsAny<int>()))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var tree = TestObject.TaxonTree;
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            tree.Should().BeNull();
        }
    }
}