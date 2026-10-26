using SOS.Api.Contracts.Common.Geo;
using ContractGeographicsFilterDto = SOS.Api.Contracts.Observations.Requests.GeographicsFilterDto;
using ContractOccurrenceStatusFilterValuesDto = SOS.Api.Contracts.Observations.Requests.OccurrenceStatusFilterValuesDto;
using ContractSearchFilterAggregationInternalDto = SOS.Api.Contracts.Aggregations.Requests.SearchFilterAggregationInternalDto;
using ContractTaxonFilterDto = SOS.Api.Contracts.Observations.Requests.TaxonFilterDto;
using FizzWare.NBuilder;
using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiClient;

[Collection(TestCollection.Name)]
public class SosObservationAggregationsClientIntegrationTests : ElasticsearchTestBase
{
    public SosObservationAggregationsClientIntegrationTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }    

    [Fact]
    public async Task ObservationAggregationsClient_TaxonAggregationInternalAsync_ReturnsPagedTaxonAggregation()
    {
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All()
                .HaveValuesFromPredefinedObservations()
                .With(observation => observation.TaxonId = TaxonIds.Svartmes)
                .With(observation => observation.NotPresent = false)
                .With(observation => observation.NotRecovered = false)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var client = TestFixture.CreateSosObservationAggregationsClient();        

        var result = await client.TaxonAggregationInternalAsync(
            CreateAggregationBoundingBoxFilter(TaxonIds.Svartmes),
            new TaxonAggregationInternalOptions { Take = 10 },
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result!.Records.Should().Contain(record =>
            record.TaxonId == TaxonIds.Svartmes &&
            record.ObservationCount == 3);
    }   

    private static ContractSearchFilterAggregationInternalDto CreateAggregationBoundingBoxFilter(int taxonId)
    {
        return new ContractSearchFilterAggregationInternalDto
        {
            OccurrenceStatus = ContractOccurrenceStatusFilterValuesDto.Present,
            Taxon = new ContractTaxonFilterDto
            {
                Ids = [taxonId]
            },
            Geographics = new ContractGeographicsFilterDto
            {
                BoundingBox = new LatLonBoundingBoxDto
                {
                    TopLeft = new LatLonCoordinateDto
                    {
                        Longitude = 10.0,
                        Latitude = 70.0
                    },
                    BottomRight = new LatLonCoordinateDto
                    {
                        Longitude = 25.0,
                        Latitude = 55.0
                    }
                }
            }
        };
    }   
}
