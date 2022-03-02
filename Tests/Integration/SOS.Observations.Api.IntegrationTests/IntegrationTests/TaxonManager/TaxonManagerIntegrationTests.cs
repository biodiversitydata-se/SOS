using FluentAssertions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;
using SOS.Observations.Api.IntegrationTests.Utils;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.TaxonManager
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class TaxonManagerIntegrationTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public TaxonManagerIntegrationTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public void Taxon_tree_root_is_biota()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var taxonTree = _fixture.TaxonManager.TaxonTree;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            taxonTree.Root.ScientificName.Should().Be("Biota");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public void Test_topological_sort()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var taxonTree = _fixture.TaxonManager.TaxonTree;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            taxonTree.CreateTopologicalSort();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            taxonTree.Root.ScientificName.Should().Be("Biota");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public void Taxon_cycle_detection()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var taxonTree = _fixture.TaxonManager.TaxonTree;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var cycles = TaxonTreeCyclesDetectionUtil.CheckForCycles(taxonTree);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            cycles.Count.Should().Be(0);
        }
    }
}