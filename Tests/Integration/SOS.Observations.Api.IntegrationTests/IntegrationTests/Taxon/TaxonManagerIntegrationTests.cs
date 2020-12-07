using FluentAssertions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.Taxon
{
    public class TaxonManagerIntegrationTests : IClassFixture<ObservationApiIntegrationTestFixture>
    {
        private readonly ObservationApiIntegrationTestFixture _fixture;

        public TaxonManagerIntegrationTests(ObservationApiIntegrationTestFixture apiTestFixture)
        {
            _fixture = apiTestFixture;
        }

        [Fact]
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