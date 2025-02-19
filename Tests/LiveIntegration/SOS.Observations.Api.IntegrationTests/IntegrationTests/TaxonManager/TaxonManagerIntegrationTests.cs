﻿using FluentAssertions;
using SOS.Lib.Factories;
using SOS.Lib.Helpers;
using SOS.Observations.Api.LiveIntegrationTests.Fixtures;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Observations.Api.LiveIntegrationTests.IntegrationTests.TaxonManager
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
        public async Task Taxon_tree_root_is_biota()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var taxonTree = await _fixture.TaxonManager.GetTaxonTreeAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            taxonTree.Root.ScientificName.Should().Be("Biota");
        }

        //[Fact]
        //[Trait("Category", "ApiIntegrationTest")]
        //public void Test_topological_sort()
        //{
        //    //-----------------------------------------------------------------------------------------------------------
        //    // Arrange
        //    //-----------------------------------------------------------------------------------------------------------
        //    var taxonTree = _fixture.TaxonManager.TaxonTree;            

        //    //-----------------------------------------------------------------------------------------------------------
        //    // Act
        //    //-----------------------------------------------------------------------------------------------------------
        //    var topoSort = taxonTree.CreateTopologicalSort();

        //    //-----------------------------------------------------------------------------------------------------------
        //    // Assert
        //    //-----------------------------------------------------------------------------------------------------------
        //    taxonTree.Root.ScientificName.Should().Be("Biota");
        //}

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Taxon_cycle_detection()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var taxonTree = await _fixture.TaxonManager.GetTaxonTreeAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var cycles = TaxonTreeCyclesDetectionHelper.CheckForCycles(taxonTree);
            string cyclesDescription = TaxonTreeCyclesDetectionHelper.GetCyclesDescription(cycles);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            cyclesDescription.Should().Be("No cycles detected");
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Test_QuickGraph_DepthFirstSearch()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var taxonTree = await _fixture.TaxonManager.GetTaxonTreeAsync();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            TaxonTreeFactory.TestQuickGraphDfs(taxonTree.TreeNodeById.Values);
        }

        //[Fact]
        //[Trait("Category", "ApiIntegrationTest")]
        //public void Get_TaxonCategories_From_TaxonTree()
        //{
        //    //-----------------------------------------------------------------------------------------------------------
        //    // Arrange
        //    //-----------------------------------------------------------------------------------------------------------
        //    var taxonTree = _fixture.TaxonManager.TaxonTree;

        //    //-----------------------------------------------------------------------------------------------------------
        //    // Act
        //    //-----------------------------------------------------------------------------------------------------------
        //    var categories = TaxonCategoryHelper.GetTaxonCategories(taxonTree);
        //    var edges = TaxonCategoryHelper.GetTaxonCategoryEdges(categories);
        //    string strMermaid = TaxonCategoryHelper.CreateMermaidDiagram(edges);
        //    string strGraphViz = TaxonCategoryHelper.CreateGraphVizDiagram(edges);

        //    //-----------------------------------------------------------------------------------------------------------
        //    // Assert
        //    //-----------------------------------------------------------------------------------------------------------
        //    categories.Should().NotBeNull();
        //}

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Create_taxon_Graphviz_diagram()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var taxonTree = await _fixture.TaxonManager.GetTaxonTreeAsync();
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

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Create_taxon_Mermaid_diagram()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var taxonTree = await _fixture.TaxonManager.GetTaxonTreeAsync();
            //var taxonIds = new List<int>() { 261815, 261806 };
            var taxonIds = new List<int>() { 222474, 1016470, 221107, 1006157, 2002715 };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var strMermaid = TaxonRelationDiagramHelper.CreateMermaidFormatRepresentation(
                taxonTree,
                taxonIds,
                TaxonRelationDiagramHelper.TaxonRelationsTreeIterationMode.BothParentsAndChildren,
                true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            strMermaid.Should().NotBeNullOrEmpty();
        }
    }
}