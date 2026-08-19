using ContractGeographicsFilterDto = SOS.Api.Contracts.Observations.Requests.GeographicsFilterDto;
using ContractLatLonBoundingBoxDto = SOS.Api.Contracts.Common.Geo.LatLonBoundingBoxDto;
using ContractLatLonCoordinateDto = SOS.Api.Contracts.Common.Geo.LatLonCoordinateDto;
using ContractOccurrenceStatusFilterValuesDto = SOS.Api.Contracts.Observations.Requests.OccurrenceStatusFilterValuesDto;
using ContractOutputFormatDto = SOS.Api.Contracts.Observations.Requests.OutputFormatDto;
using ContractSearchFilterAggregationInternalDto = SOS.Api.Contracts.Aggregations.Requests.SearchFilterAggregationInternalDto;
using ContractSearchFilterDto = SOS.Api.Contracts.Observations.Requests.SearchFilterDto;
using ContractSearchFilterInternalBaseDto = SOS.Api.Contracts.Observations.Requests.SearchFilterInternalBaseDto;
using ContractSearchFilterInternalDto = SOS.Api.Contracts.Observations.Requests.SearchFilterInternalDto;
using ContractSearchSortOrder = SOS.Api.Contracts.Observations.Requests.SearchSortOrder;
using ContractTaxonFilterDto = SOS.Api.Contracts.Observations.Requests.TaxonFilterDto;
using FizzWare.NBuilder;
using MongoDB.Driver.GeoJsonObjectModel;
using SOS.Api.Client.Requests;
using SOS.Api.Client.Exceptions;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiClient;

[Collection(TestCollection.Name)]
public class SosObservationsClientIntegrationTests : TestBase
{
    public SosObservationsClientIntegrationTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task ObservationsBySearchInternalAsync_ReturnsPagedResponse_FromInternalSearchEndpoint()
    {
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(5)
            .All()
                .HaveValuesFromPredefinedObservations()
                .With(observation => observation.TaxonId = TaxonIds.Skata)
                .With(observation => observation.NotPresent = false)
                .With(observation => observation.NotRecovered = false)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var client = TestFixture.CreateSosObservationsClient();
        var searchFilter = CreateTaxonFilter(TaxonIds.Skata);

        var result = await client.ObservationsBySearchInternalAsync(
            searchFilter,
            new ObservationSearchOptions
            {
                Skip = 1,
                Take = 2
            },
            TestContext.Current.CancellationToken);
        
        result.Skip.Should().Be(1);
        result.Take.Should().Be(2);
        result.TotalCount.Should().Be(5);
        result.Records.Should().HaveCount(2);
        result.Records.First().Taxon.Id.Should().Be(TaxonIds.Skata);        
    }

    [Fact]
    public async Task ObservationsByCursorAsync_ReturnsCursorMetadata_FromSearchByCursorEndpoint()
    {
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All()
                .HaveValuesFromPredefinedObservations()
                .With(observation => observation.TaxonId = TaxonIds.Blåmes)
                .With(observation => observation.NotPresent = false)
                .With(observation => observation.NotRecovered = false)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var client = TestFixture.CreateSosObservationsClient();
        var searchFilter = CreatePublicTaxonFilter(TaxonIds.Blåmes);

        var result = await client.ObservationsByCursorAsync(
            searchFilter,
            new SearchByCursorOptions
            {
                Take = 2,
                SortBy = "modified",
                SortOrder = ContractSearchSortOrder.Asc
            },
            TestContext.Current.CancellationToken);

        result.Take.Should().Be(2);
        result.TotalCount.Should().Be(3);
        result.Records.Should().HaveCount(2);
        result.NextCursor.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task AdaptiveSearchLocationsInternalAsync_ReturnsGeoJsonAndResponseHeaders_FromAdaptiveLocationsEndpoint()
    {
        var verbatimObservations = CreatePointObservations(
            count: 3,
            siteIdStart: 450000,
            taxonId: TaxonIds.Talgoxe,
            coordinateFactory: index => (13.850 + index * 0.001, 58.380 + index * 0.001));
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var client = TestFixture.CreateSosObservationsClient();
        var searchFilter = CreateBoundingBoxFilter(TaxonIds.Talgoxe, 13.84, 58.40, 13.90, 58.35);

        var result = await client.AdaptiveSearchLocationsInternalAsync(
            searchFilter,
            new AdaptiveSearchLocationsOptions
            {
                ObservationsLimit = 100,
                Cluster = false,
                ReturnInputBBox = true,
                UseCache = false
            },
            TestContext.Current.CancellationToken);

        result.GeoJson.Should().Contain("\"FeatureCollection\"");
        result.ResultType.Should().Be("locations");
        result.ObservationsTotalCount.Should().Be(3);
        result.FeatureCount.Should().Be(3);
    }

    [Fact]
    public async Task ObservationsBySearchInternalAsync_ThrowsSosApiException_WhenApiReturnsBadRequest()
    {
        var client = TestFixture.CreateSosObservationsClient();

        var exception = await Assert.ThrowsAsync<SosApiException>(() => client.ObservationsBySearchInternalAsync(
            new ContractSearchFilterInternalDto(),
            new ObservationSearchOptions
            {
                Take = 5001,
                OutputFormat = ContractOutputFormatDto.Json
            },
            TestContext.Current.CancellationToken));

        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.ResponseBody.Should().NotBeNullOrWhiteSpace();
        exception.RequestUri?.Query.Should().Contain("take=5001");
    }

    [Fact]
    public async Task ObservationService_CountInternalAsync_ReturnsMatchCount()
    {
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(4)
            .All()
                .HaveValuesFromPredefinedObservations()
                .With(observation => observation.TaxonId = TaxonIds.Duvhök)
                .With(observation => observation.NotPresent = false)
                .With(observation => observation.NotRecovered = false)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var client = TestFixture.CreateSosObservationsClient();

        var result = await client.CountInternalAsync(
            CreateCountFilter(TaxonIds.Duvhök),
            cancellationToken: TestContext.Current.CancellationToken);

        result.Should().Be(4);
    }
   
    private static ContractSearchFilterInternalDto CreateTaxonFilter(int taxonId)
    {
        return new ContractSearchFilterInternalDto
        {
            OccurrenceStatus = ContractOccurrenceStatusFilterValuesDto.Present,
            Taxon = new ContractTaxonFilterDto
            {
                Ids = [taxonId]
            }
        };
    }

    private static ContractSearchFilterInternalBaseDto CreateCountFilter(int taxonId)
    {
        return new ContractSearchFilterInternalBaseDto
        {
            OccurrenceStatus = ContractOccurrenceStatusFilterValuesDto.Present,
            Taxon = new ContractTaxonFilterDto
            {
                Ids = [taxonId]
            }
        };
    }   

    private static ContractSearchFilterDto CreatePublicTaxonFilter(int taxonId)
    {
        return new ContractSearchFilterDto
        {
            OccurrenceStatus = ContractOccurrenceStatusFilterValuesDto.Present,
            Taxon = new ContractTaxonFilterDto
            {
                Ids = [taxonId]
            }
        };
    }

    private static ContractSearchFilterInternalDto CreateBoundingBoxFilter(
        int taxonId,
        double left,
        double top,
        double right,
        double bottom)
    {
        var searchFilter = CreateTaxonFilter(taxonId);
        searchFilter.Geographics = new ContractGeographicsFilterDto
        {
            BoundingBox = new ContractLatLonBoundingBoxDto
            {
                TopLeft = new ContractLatLonCoordinateDto
                {
                    Longitude = left,
                    Latitude = top
                },
                BottomRight = new ContractLatLonCoordinateDto
                {
                    Longitude = right,
                    Latitude = bottom
                }
            }
        };

        return searchFilter;
    }

    private static IList<ArtportalenObservationVerbatim> CreatePointObservations(
        int count,
        int siteIdStart,
        int taxonId,
        Func<int, (double Longitude, double Latitude)> coordinateFactory)
    {
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(count)
            .All()
                .HaveValuesFromPredefinedObservations()
                .With(observation => observation.TaxonId = taxonId)
                .With(observation => observation.NotPresent = false)
                .With(observation => observation.NotRecovered = false)
            .Build();

        for (var index = 0; index < observations.Count; index++)
        {
            var coordinate = coordinateFactory(index);
            observations[index].Site.HasGeometry = false;
            observations[index].Site.Id = siteIdStart + index;
            observations[index].Site.Point = new GeoJsonPoint<GeoJson2DCoordinates>(
                new GeoJson2DCoordinates(coordinate.Longitude, coordinate.Latitude));
        }

        return observations;
    }
}
