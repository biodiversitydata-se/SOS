using FluentAssertions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.Taxon
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
    }
}