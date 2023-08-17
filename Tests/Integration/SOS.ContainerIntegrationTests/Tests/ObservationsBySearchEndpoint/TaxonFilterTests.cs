using FizzWare.NBuilder;
using SOS.ContainerIntegrationTests.Setup;
using SOS.ContainerIntegrationTests.TestData.TestDataBuilder;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.ContainerIntegrationTests.Tests.ObservationsBySearchEndpoint;

/// <summary>
/// Integration tests for the get Environment endpoint.
/// </summary>
[Collection(IntegrationTestsCollection.Name)]
public class TaxonFilterTests : IntegrationTestsBase
{
    public TaxonFilterTests(IntegrationTestsFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    [Trait("Category", "AutomaticIntegrationTest")]
    public async Task TestRedListCategoriesFilter()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange - Create verbatim observations
        //-----------------------------------------------------------------------------------------------------------            
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All()
                .HaveValuesFromPredefinedObservations()
            .TheFirst(20)
                .HaveRedlistedTaxonId("CR") // Critically Endangered (Akut hotad)
            .TheNext(20)
                .HaveRedlistedTaxonId("EN") // Endangered (Starkt hotad)
            .TheNext(20)
                .HaveRedlistedTaxonId("VU") // Vulnerable (Sårbar)
            .TheNext(20)
                .HaveRedlistedTaxonId("NT") // Near Threatened (Nära hotad)
            .TheNext(20)
                .HaveRedlistedTaxonId(null) // Not redlisted taxa
            .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var searchFilter = new SearchFilterDto {
            Taxon = new TaxonFilterDto {
                RedListCategories = new List<string> { "CR", "EN", "VU" }
            }
        };

        //-----------------------------------------------------------------------------------------------------------
        // Act - Get observation by occurrenceId
        //-----------------------------------------------------------------------------------------------------------
        var apiClient = TestFixture.CreateApiClient();
        JsonContent content = JsonContent.Create(searchFilter);
        var response = await apiClient.PostAsync($"/observations/search", content);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------            
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(60);
    }
}