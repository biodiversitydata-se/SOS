using FizzWare.NBuilder;
using MongoDB.Driver.GeoJsonObjectModel;
using NetTopologySuite.Features;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Enums;
using SOS.Lib.Gis;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Helpers;
using SOS.Observations.Api.IntegrationTests.Models;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Filter;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class AdaptiveSearchTests : TestBase
{
    public AdaptiveSearchTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task AdaptiveSearchInternal_ReturnsObservations_WhenTotalCountBelowObservationsLimit()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()            
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterInternalDto();

        // Act
        var response = await apiClient.PostAsync(
            "/observations/internal/adaptivesearch?observationsLimit=100",
            JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);        
        var strGeoJson = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        FeatureCollection featureCollection = GeoJsonHelper.ReadGeoJsonFromString(strGeoJson);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);        
        featureCollection.Count.Should().Be(100);
        response.GetHeaderValue("X-Result-Type").Should().Be("observations");
        response.GetHeaderValue("X-Observations-TotalCount").Should().Be("100");
        response.GetHeaderValue("X-Observations-Count").Should().Be("100");        
        response.GetHeaderValue("X-Result-Count").Should().Be("100");
    }

    [Fact]
    public async Task AdaptiveSearchInternal_ReturnsGridAggregation_WhenTotalCountExceedsObservationsLimit()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterInternalDto();

        // Act
        var response = await apiClient.PostAsync(
            "/observations/internal/adaptivesearch?observationsLimit=50",
            JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var strGeoJson = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        FeatureCollection featureCollection = GeoJsonHelper.ReadGeoJsonFromString(strGeoJson);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        featureCollection.Count.Should().BeGreaterThanOrEqualTo(1, because: "at least one grid cell is returned");
        response.GetHeaderValue("X-Result-Type").Should().Be("geogrid");
        response.GetHeaderValue("X-Observations-TotalCount").Should().Be("100");
        Convert.ToInt32(response.GetHeaderValue("X-Zoom")).Should().BeGreaterThanOrEqualTo(1, because: "zoom should be greater than or equal to 1");
        Convert.ToInt32(response.GetHeaderValue("X-Result-Count")).Should().BeGreaterThanOrEqualTo(1, because: "at least one grid cell is returned");
    }    

    [Fact]
    public async Task AdaptiveSearchInternal_ReturnsGeoJsonGroupedByLocationObject_WhenGeoJsonByLocationIsRequestedWithAllFields()
    {
        // Arrange
        var (verbatimObservations, polygonSiteId1, polygonSiteId2, pointSiteId1) = CreateTestObservations();
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterInternalDto()
        {
            Output = new OutputFilterExtendedDto
            {
                FieldSet = Lib.Enums.OutputFieldSet.All
            }
        };

        // Act
        var response = await apiClient.PostAsync(
            $"/observations/internal/adaptivesearch?observationsLimit=10&outputFormat={OutputFormatDto.GeoJsonByLocation}&createPolygons={CreatePolygonsMode.Yes}",
            JsonContent.Create(searchFilter));
        var responseString = await response.Content.ReadAsStringAsync();
        var featureCollection = JsonSerializer.Deserialize<GeoJsonResponse<ObservationsByLocation>>(responseString, JsonSerializerOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.GetHeaderValue("X-Result-Type").Should().Be("observations");
        response.GetHeaderValue("X-Observations-TotalCount").Should().Be("6");        
        response.GetHeaderValue("X-Result-Count").Should().Be("3");
        featureCollection!.Features.Count().Should().Be(3);

        var polygonFeature = featureCollection.Features
            .First(feature => feature.Id == $"urn:lsid:artportalen.se:site:{polygonSiteId1}");
        polygonFeature.Geometry.GeometryType.Should().Be("Polygon");        

        var observationsByLocation = polygonFeature.Properties;
        observationsByLocation.Location.LocationId.Should().Be($"urn:lsid:artportalen.se:site:{polygonSiteId1}");
        observationsByLocation.ObservationCount.Should().Be(2);
        observationsByLocation.TaxaCount.Should().BeGreaterThanOrEqualTo(1);
        observationsByLocation.Observations.Count().Should().Be(2);
        observationsByLocation.Observations.First().Occurrence.OccurrenceId.Should().NotBeNullOrEmpty();
        observationsByLocation.Observations.Should().OnlyContain(obs => obs.Location == null, because: "the location information should not be included in the observations");

        var pointFeature = featureCollection.Features
            .First(feature => feature.Id == $"urn:lsid:artportalen.se:site:{pointSiteId1}");
        pointFeature.Geometry.GeometryType.Should().Be("Point");
        var pointObservationsByLocation = pointFeature.Properties;
        pointObservationsByLocation.ObservationCount.Should().Be(2);
        pointObservationsByLocation.Observations.Should().OnlyContain(obs => obs.Location == null, because: "the location information should not be included in the observations");
    }

    [Fact]
    public async Task AdaptiveSearchInternal_ReturnsFlatGeoJsonGroupedByLocationObject_WhenGeoJsonByLocationIsRequestedWithAllFields()
    {
        // Arrange
        var (verbatimObservations, polygonSiteId1, polygonSiteId2, pointSiteId1) = CreateTestObservations();
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterInternalDto()
        {
            Output = new OutputFilterExtendedDto
            {
                FieldSet = Lib.Enums.OutputFieldSet.All
            }
        };
        
        // Act
        var response = await apiClient.PostAsync(
            $"/observations/internal/adaptivesearch?observationsLimit=10&outputFormat={OutputFormatDto.GeoJsonByLocationFlat}&geoJsonPropertyLabelType={PropertyLabelType.PropertyName}&createPolygons={CreatePolygonsMode.Yes}",
            JsonContent.Create(searchFilter));
        var responseString = await response.Content.ReadAsStringAsync();
        var featureCollection = JsonSerializer.Deserialize<GeoJsonResponse<GeoJsonFlatObservationsByLocation>>(responseString, JsonSerializerOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.GetHeaderValue("X-Result-Type").Should().Be("observations");
        response.GetHeaderValue("X-Observations-TotalCount").Should().Be("6");        
        response.GetHeaderValue("X-Result-Count").Should().Be("3");
        featureCollection!.Features.Count().Should().Be(3);

        var polygonFeature = featureCollection.Features
            .First(feature => feature.Id == $"urn:lsid:artportalen.se:site:{polygonSiteId1}");
        polygonFeature.Geometry.GeometryType.Should().Be("Polygon");

        var observationsByLocation = polygonFeature.Properties;
        observationsByLocation.Location.LocationId.Should().Be($"urn:lsid:artportalen.se:site:{polygonSiteId1}");
        observationsByLocation.ObservationCount.Should().Be(2);
        observationsByLocation.TaxaCount.Should().BeGreaterThanOrEqualTo(1);
        observationsByLocation.Observations.Count().Should().Be(2);
        observationsByLocation.Observations.First().OccurrenceId.Should().NotBeNullOrEmpty();

        var pointFeature = featureCollection.Features
            .First(feature => feature.Id == $"urn:lsid:artportalen.se:site:{pointSiteId1}");
        pointFeature.Geometry.GeometryType.Should().Be("Point");
        var pointObservationsByLocation = pointFeature.Properties;
        pointObservationsByLocation.ObservationCount.Should().Be(2);
    }    

    [Fact]
    public async Task AdaptiveSearchInternal_ReturnsJson_WhenJsonIsRequestedWithAllFields()
    {
        // Arrange
        var (verbatimObservations, polygonSiteId1, polygonSiteId2, pointSiteId1) = CreateTestObservations();
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterInternalDto()
        {
            Output = new OutputFilterExtendedDto
            {
                FieldSet = Lib.Enums.OutputFieldSet.All
            }
        };

        // Act
        var response = await apiClient.PostAsync(
            $"/observations/internal/adaptivesearch?observationsLimit=10&outputFormat={OutputFormatDto.Json}&geoJsonPropertyLabelType={PropertyLabelType.PropertyName}",
            JsonContent.Create(searchFilter));
        var responseString = await response.Content.ReadAsStringAsync();
        var resultDto = JsonSerializer.Deserialize<AdaptiveSearchResultDto<Observation>>(responseString, JsonSerializerOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        resultDto.ResultType.Should().Be(AdaptiveSearchResultTypeDto.Observations);
        resultDto.TotalObservationsCount.Should().Be(6);
        resultDto.ResultCount.Should().Be(6);
        resultDto.ObservationsResult.Count().Should().Be(6);
        resultDto.ObservationsResult.First().DatasetName.Should().Be("Artportalen");
    }

    [Fact]
    public async Task AdaptiveSearchInternal_ReturnsJsonGroupedByLocation_WhenJsonByLocationIsRequestedWithAllFields()
    {
        // Arrange
        var (verbatimObservations, polygonSiteId1, polygonSiteId2, pointSiteId1) = CreateTestObservations();
        var processedObservations = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterInternalDto()
        {
            Output = new OutputFilterExtendedDto
            {
                FieldSet = Lib.Enums.OutputFieldSet.All
            }
        };

        // Act
        var response = await apiClient.PostAsync(
            $"/observations/internal/adaptivesearch?observationsLimit=10&outputFormat={OutputFormatDto.JsonByLocation}&createPolygons={CreatePolygonsMode.Yes}",
            JsonContent.Create(searchFilter));
        var responseString = await response.Content.ReadAsStringAsync();
        var resultDto = JsonSerializer.Deserialize<AdaptiveSearchResultByLocationDto<Observation, Location>>(responseString, JsonSerializerOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        resultDto.ResultType.Should().Be(AdaptiveSearchResultTypeDto.Observations);
        resultDto.TotalObservationsCount.Should().Be(6);
        resultDto.ObservationsCount.Should().Be(6);
        resultDto.ResultCount.Should().Be(3);
        resultDto.LocationResult.Count().Should().Be(3);

        var polygonLocationDto = resultDto.LocationResult.First(location => location.Location.LocationId == $"urn:lsid:artportalen.se:site:{polygonSiteId1}");
        polygonLocationDto.Location.LocationId.Should().Be($"urn:lsid:artportalen.se:site:{polygonSiteId1}");       
        polygonLocationDto.Geometry.GeometryType.Should().Be("Polygon");
        polygonLocationDto.ObservationCount.Should().Be(2);
        polygonLocationDto.TaxaCount.Should().BeGreaterThanOrEqualTo(1);
        polygonLocationDto.Observations.Count().Should().Be(2);
        polygonLocationDto.Observations.First().Occurrence.OccurrenceId.Should().NotBeNullOrEmpty();
        polygonLocationDto.Observations.Should().OnlyContain(obs => obs.Location == null, because: "the location information should not be included in the observations");        

        var pointLocationDto = resultDto.LocationResult.First(location => location.Location.LocationId == $"urn:lsid:artportalen.se:site:{pointSiteId1}");
        pointLocationDto.Location.LocationId.Should().Be($"urn:lsid:artportalen.se:site:{pointSiteId1}");
        pointLocationDto.Geometry.GeometryType.Should().Be("Point");
        pointLocationDto.ObservationCount.Should().Be(2);
        pointLocationDto.TaxaCount.Should().BeGreaterThanOrEqualTo(1);
        pointLocationDto.Observations.Count().Should().Be(2);
        pointLocationDto.Observations.First().Occurrence.OccurrenceId.Should().NotBeNullOrEmpty();
        pointLocationDto.Observations.Should().OnlyContain(obs => obs.Location == null, because: "the location information should not be included in the observations");
    }

    [Fact]
    public async Task AdaptiveSearchInternal_ReturnsGeoJsonGroupedByLocation_WhenDwcObservationsWithFootprintWktAreUsed()
    {
        // Arrange
        const string locationId1 = "test-location-dwc-1";
        const string locationId2 = "test-location-dwc-2";
        const string polygonWkt1 = "POLYGON((18.0 59.3, 18.1 59.3, 18.1 59.4, 18.0 59.4, 18.0 59.3))";
        const string polygonWkt2 = "POLYGON((17.5 58.5, 17.6 58.5, 17.6 58.6, 17.5 58.6, 17.5 58.5))";

        var verbatimObservations = Builder<DwcObservationVerbatim>.CreateListOfSize(6)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(2)
                .With(o => o.LocationID = locationId1)
                .With(o => o.FootprintWKT = polygonWkt1)
            .TheNext(2)
                .With(o => o.LocationID = locationId2)
                .With(o => o.FootprintWKT = polygonWkt2)
            .TheNext(2)
                .With(o => o.LocationID = null)                
            .Build();
        var processedObservations = ProcessFixture.ProcessObservations(verbatimObservations);        
        await ProcessFixture.AddObservationsToElasticsearchAsync(processedObservations, true);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterInternalDto();

        // Act
         var response = await apiClient.PostAsync(
            $"/observations/internal/adaptivesearch?observationsLimit=10&outputFormat={OutputFormatDto.GeoJsonByLocation}",
            JsonContent.Create(searchFilter));
        var responseString = await response.Content.ReadAsStringAsync();
        var featureCollection = JsonSerializer.Deserialize<GeoJsonResponse<ObservationsByLocation>>(responseString, JsonSerializerOptions);        

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.GetHeaderValue("X-Result-Type").Should().Be("observations");
        response.GetHeaderValue("X-Observations-TotalCount").Should().Be("6");        
        response.GetHeaderValue("X-Result-Count").Should().Be("4");
        featureCollection.Features.Count().Should().Be(4);

        var feature1 = featureCollection.Features
            .First(feature => feature.Id == locationId1);
        feature1.Properties.Location.LocationId.Should().Be(locationId1);
        feature1.Properties.ObservationCount.Should().Be(2);
        feature1.Properties.TaxaCount.Should().BeGreaterThanOrEqualTo(1);
    }

    private (IList<ArtportalenObservationVerbatim> Observations, int PolygonSiteId1, int PolygonSiteId2, int PointSiteId1) CreateTestObservations()
    {
        const int polygonSiteId1 = 140228;
        const int polygonSiteId2 = 140229;
        const int pointSiteId1 = 140230;

        // Polygon 1 in Västergötland (near Skövde): irregular shape, 7 coordinates,
        // centroid ~(13.848, 58.385)
        var polygon1 = new GeoJsonPolygon<GeoJson2DCoordinates>(
            new GeoJsonPolygonCoordinates<GeoJson2DCoordinates>(
                new GeoJsonLinearRingCoordinates<GeoJson2DCoordinates>([
                    new GeoJson2DCoordinates(13.8455027, 58.3812164),
                    new GeoJson2DCoordinates(13.8512457, 58.3802266),
                    new GeoJson2DCoordinates(13.8568313, 58.3869489),
                    new GeoJson2DCoordinates(13.8523355, 58.3883689),
                    new GeoJson2DCoordinates(13.8445808, 58.3885298),
                    new GeoJson2DCoordinates(13.841, 58.385),
                    new GeoJson2DCoordinates(13.8472681, 58.3831367),
                    new GeoJson2DCoordinates(13.8455027, 58.3812164), // closing
                ])));
        var centroid1 = new GeoJsonPoint<GeoJson2DCoordinates>(new GeoJson2DCoordinates(13.848, 58.385));

        // Polygon 2 in Västergötland (~1.3 km east of polygon 1): irregular shape, 7 coordinates,
        // centroid ~(13.870, 58.385)
        var polygon2 = new GeoJsonPolygon<GeoJson2DCoordinates>(
            new GeoJsonPolygonCoordinates<GeoJson2DCoordinates>(
                new GeoJsonLinearRingCoordinates<GeoJson2DCoordinates>([
                    new GeoJson2DCoordinates(13.8681485, 58.3827676),
                    new GeoJson2DCoordinates(13.8718573, 58.3824537),
                    new GeoJson2DCoordinates(13.8739027, 58.3843509),
                    new GeoJson2DCoordinates(13.8742961, 58.3872788),
                    new GeoJson2DCoordinates(13.8707623, 58.3874835),
                    new GeoJson2DCoordinates(13.8643857, 58.386044),
                    new GeoJson2DCoordinates(13.8633016, 58.3833026),
                    new GeoJson2DCoordinates(13.8681485, 58.3827676), // closing
                ])));
        var centroid2 = new GeoJsonPoint<GeoJson2DCoordinates>(new GeoJson2DCoordinates(13.870, 58.385));

        // Point near the polygons for HasGeometry=false observations
        var point1 = new GeoJsonPoint<GeoJson2DCoordinates>(new GeoJson2DCoordinates(13.862, 58.390));

        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(6)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(2)
                .With(o => o.Site.HasGeometry = true)
                .With(o => o.Site.Id = polygonSiteId1)
                .With(o => o.Site.PointWithBuffer = polygon1)
                .With(o => o.Site.Point = centroid1)
                .With(o => o.Site.Province.Name = "Västergötland")
                .With(o => o.Site.Province.FeatureId = "9")
                .With(o => o.Site.County.Name = "Västra Götaland")
                .With(o => o.Site.County.FeatureId = "14")
                .With(o => o.Site.Municipality.Name = "Skövde")
                .With(o => o.Site.Municipality.FeatureId = "1496")
                .With(o => o.Site.Parish.Name = "Skövde")
                .With(o => o.Site.Parish.FeatureId = "2030")
            .TheNext(2)
                .With(o => o.Site.HasGeometry = true)
                .With(o => o.Site.Id = polygonSiteId2)
                .With(o => o.Site.PointWithBuffer = polygon2)
                .With(o => o.Site.Point = centroid2)
                .With(o => o.Site.Province.Name = "Västergötland")
                .With(o => o.Site.Province.FeatureId = "9")
                .With(o => o.Site.County.Name = "Västra Götaland")
                .With(o => o.Site.County.FeatureId = "14")
                .With(o => o.Site.Municipality.Name = "Skövde")
                .With(o => o.Site.Municipality.FeatureId = "1496")
                .With(o => o.Site.Parish.Name = "Skövde")
                .With(o => o.Site.Parish.FeatureId = "2030")
            .TheNext(2)
                .With(o => o.Site.HasGeometry = false)
                .With(o => o.Site.Id = pointSiteId1)
                .With(o => o.Site.Point = point1)
                .With(o => o.Site.Province.Name = "Västergötland")
                .With(o => o.Site.Province.FeatureId = "9")
                .With(o => o.Site.County.Name = "Västra Götaland")
                .With(o => o.Site.County.FeatureId = "14")
                .With(o => o.Site.Municipality.Name = "Skövde")
                .With(o => o.Site.Municipality.FeatureId = "1496")
                .With(o => o.Site.Parish.Name = "Skövde")
                .With(o => o.Site.Parish.FeatureId = "2030")
            .Build();

        return (observations, polygonSiteId1, polygonSiteId2, pointSiteId1);
    }
}
