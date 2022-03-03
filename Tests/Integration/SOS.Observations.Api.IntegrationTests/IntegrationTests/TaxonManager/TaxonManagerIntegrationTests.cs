using FluentAssertions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;
using SOS.Observations.Api.IntegrationTests.Utils;
using SOS.Lib.Helpers;
using System.Collections.Generic;

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

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public void Create_taxon_Graphviz_diagram()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var taxonTree = _fixture.TaxonManager.TaxonTree;
            //var taxonIds = new List<int>() { 261815, 261806 };
            var taxonIds = new List<int>() { 222474, 1016470, 221107, 1006157, 2002715 };
            
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var strGraphviz = TaxonRelationDiagramHelper.CreateGraphvizFormatRepresentation(
                taxonTree,
                taxonIds,
                TaxonRelationDiagramHelper.TaxonRelationsTreeIterationMode.BothParentsAndChildren,
                true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            strGraphviz.Should().NotBeNullOrEmpty();
        }
    }
}