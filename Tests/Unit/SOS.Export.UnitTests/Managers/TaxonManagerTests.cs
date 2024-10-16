using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using Moq;
using SOS.Lib.Cache;
using SOS.Lib.Managers;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Resource.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            _taxonListRepositoryMock = new Mock<ITaxonListRepository>();
            _loggerMock = new Mock<ILogger<TaxonManager>>();
        }

        private readonly Mock<ITaxonRepository> _processedTaxonRepositoryMock;
        private readonly Mock<ITaxonListRepository> _taxonListRepositoryMock;
        private readonly Mock<ILogger<TaxonManager>> _loggerMock;

        /// <summary>
        ///     Return object to be tested
        /// </summary>
        private TaxonManager TestObject => new TaxonManager(
            _processedTaxonRepositoryMock.Object,
            _taxonListRepositoryMock.Object,
            new ClassCache<TaxonTree<IBasicTaxon>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<TaxonTree<IBasicTaxon>>>()),
            new ClassCache<TaxonListSetsById>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<TaxonListSetsById>>()),
            _loggerMock.Object);

        /// <summary>
        ///     Test taxon tree fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task TaxonTreeFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processedTaxonRepositoryMock.Setup(pir => pir.GetAllAsync(It.IsAny<ProjectionDefinition<Taxon, BasicTaxon>>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<BasicTaxon>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var tree = await TestObject.GetTaxonTreeAsync();
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
        public async Task TaxonTreeSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _processedTaxonRepositoryMock.Setup(pir => pir.GetAllAsync(It.IsAny<ProjectionDefinition<Taxon, BasicTaxon>>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<BasicTaxon>() { new BasicTaxon { Id = 1 } });

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var tree = await TestObject.GetTaxonTreeAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            tree.Should().NotBe(null);
            tree.TreeNodeById.Count.Should().BeGreaterThan(0);
        }
    }
}