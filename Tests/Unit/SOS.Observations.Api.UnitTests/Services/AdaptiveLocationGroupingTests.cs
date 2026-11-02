using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NetTopologySuite.Geometries;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Lib.Gis;
using SOS.Observations.Api.Services.AdaptiveSearchLocations;
using System.Text.Json;
using Xunit;
using Location = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Observations.Api.UnitTests.Services;

public class AdaptiveLocationGroupingTests
{
    [Fact]
    public void SingleAggregateCopiesSummaryStateAndKeepsPresentationAndCutoffStatus()
    {
        var aggregate = MakeAggregate(7);
        var summary = aggregate.ObservationSummary;
        summary.ObservationCount = 356;
        summary.FoundCount = 356;
        summary.AllObservationsArePresent = true;
        summary.MinPresentEndYear = 1990;
        summary.MaxPresentEndYear = 2020;
        summary.PresentationStatusSummaries.Add(new() { IsPresent = true, EndYear = 1990, Count = 100 });
        summary.PresentationStatusSummaries.Add(new() { IsPresent = true, EndYear = 2020, Count = 256 });
        var factory = new AdaptiveSearchLocationFeatureFactory(new AdaptiveSearchLocationAggregator());
        var feature = factory.CreateItems(new List<CachedLocationAggregate> { aggregate }, new LocationFeatureOptions()).Single();
        var before = JsonSerializer.Serialize(LocationFeatureResponseMapper.ToProperties(feature, 2000));
        feature.ObservationSummary.Should().NotBeSameAs(summary);
        feature.ObservationSummary.FoundCount = 0;
        feature.ObservationSummary.PresentationStatusSummaries[0].Count = 0;
        feature.ObservationSummary.PresentationStatusSummaries.Clear();
        summary.FoundCount.Should().Be(356);
        summary.PresentationStatusSummaries.Should().HaveCount(2);
        summary.PresentationStatusSummaries[0].Count.Should().Be(100);
        var again = factory.CreateItems(new List<CachedLocationAggregate> { aggregate }, new LocationFeatureOptions()).Single();
        JsonSerializer.Serialize(LocationFeatureResponseMapper.ToProperties(again, 2000)).Should().Be(before);
    }

    [Fact]
    public async Task ClusterIdentityIncludesEveryIdAndIgnoresOrderingAndCase()
    {
        var first = MakeAggregate(7);
        var a = await Write(MakeCluster(first));
        first.LocationIds.Reverse();
        var b = await Write(MakeCluster(first));
        a.GetProperty("id").GetString().Should().Be(b.GetProperty("id").GetString());
        for (var i = 0; i < first.LocationIds.Count; i++) first.LocationIds[i] = first.LocationIds[i].ToUpperInvariant();
        var c = await Write(MakeCluster(first));
        c.GetProperty("id").GetString().Should().Be(a.GetProperty("id").GetString());
        first.LocationIds.Add("extra-outside-items-limit");
        var d = await Write(MakeCluster(first));
        d.GetProperty("id").GetString().Should().NotBe(a.GetProperty("id").GetString());
        a.GetProperty("id").GetString().Should().NotBeNullOrWhiteSpace();
        a.GetProperty("properties").GetProperty("locations").GetProperty("items").GetArrayLength().Should().Be(5);
        a.GetProperty("properties").GetProperty("hasMultipleLocations").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task NonClusterIdentityMatchesOriginalDistinctIdentity()
    {
        var aggregate = MakeAggregate(7);
        var factory = new AdaptiveSearchLocationFeatureFactory(new AdaptiveSearchLocationAggregator());
        var feature = factory.CreateItems(new List<CachedLocationAggregate> { aggregate }, new LocationFeatureOptions { PolygonGeometries = null, CreatePolygons = CreatePolygonsMode.No, MetersPerPixel = null }).Single();
        var json = await Write(feature);
        json.GetProperty("id").GetString().Should().Be(aggregate.DistinctLocationId);
        var properties = json.GetProperty("properties");
        properties.TryGetProperty("isCluster", out _).Should().BeFalse();
        properties.GetProperty("groupingType").GetString().Should().Be("coLocatedLocations");
        properties.GetProperty("hasMultipleLocations").GetBoolean().Should().BeTrue();
        properties.GetProperty("locations").GetProperty("items")[0].GetProperty("locationId").GetString().Should().Be("site-0");
    }

    [Fact]
    public void PolygonUnionIncludesLocationsBeyondTheFiveDisplayedItems()
    {
        var aggregate = MakeAggregate(7);
        var polygons = new Dictionary<string, Geometry>();
        for (var i = 0; i < 7; i++)
        {
            aggregate.Locations[i].Type = SOS.Lib.Enums.LocationType.Polygon;
            polygons[aggregate.Locations[i].LocationId] = new GeometryFactory().ToGeometry(new Envelope(15 + i * 0.01, 15.005 + i * 0.01, 59, 59.005));
        }
        var factory = new AdaptiveSearchLocationFeatureFactory(new AdaptiveSearchLocationAggregator());
        var feature = factory.CreateItems(new List<CachedLocationAggregate> { aggregate }, new LocationFeatureOptions { PolygonGeometries = polygons, CreatePolygons = CreatePolygonsMode.Yes, MetersPerPixel = null }).Single();
        feature.Geometry!.Covers(new Point(15.062, 59.002)).Should().BeTrue();
        LocationFeatureResponseMapper.ToProperties(feature).Locations.Items.Should().HaveCount(5);
    }

    [Fact]
    public void GeneralizedUnionUsesLargestSurfaceEvenBeyondItemLimit()
    {
        var aggregate = MakeAggregate(7);
        foreach (var location in aggregate.Locations) aggregate.GeneralizedLocationIds.Add(location.LocationId);
        aggregate.Locations[6].CoordinateUncertaintyInMeters = 10000;
        var factory = new AdaptiveSearchLocationFeatureFactory(new AdaptiveSearchLocationAggregator());
        var feature = factory.CreateItems(new List<CachedLocationAggregate> { aggregate }, new LocationFeatureOptions { PolygonGeometries = null, CreatePolygons = CreatePolygonsMode.No, MetersPerPixel = null }).Single();
        feature.Geometry!.EnvelopeInternal.Width.Should().BeGreaterThan(0.1);
        var dto = LocationFeatureResponseMapper.ToProperties(feature);
        dto.Locations.Items.Should().HaveCount(5).And.OnlyContain(i => i.IsGeneralized == true);
    }

    [Fact]
    public void GeneralizationBelongsToTheOriginalIdAndDoesNotLeakFromCachedRepresentative()
    {
        var aggregate = MakeAggregate(7);
        aggregate.GeneralizedLocationIds.Add("site-6");
        aggregate.ObservationSummary.DiffusedCount = 1;
        aggregate.Locations[6].CoordinateUncertaintyInMeters = 10000;
        var factory = new AdaptiveSearchLocationFeatureFactory(new AdaptiveSearchLocationAggregator());
        var feature = factory.CreateItems(new List<CachedLocationAggregate> { aggregate }, new LocationFeatureOptions { PolygonGeometries = null, CreatePolygons = CreatePolygonsMode.No, MetersPerPixel = null }).Single();
        var properties = LocationFeatureResponseMapper.ToProperties(feature);
        properties.Locations.Items.Should().OnlyContain(item => item.IsGeneralized == false);
        properties.ObservationSummary.StatusCounts!.DiffusedCount.Should().Be(1);
        feature.Geometry!.EnvelopeInternal.Width.Should().BeGreaterThan(0.1);
    }

    private static LocationFeature MakeCluster(CachedLocationAggregate aggregate) => new()
    {
        IsCluster = true, GroupingType = LocationGroupingType.MeterCluster,
        LocationIds = aggregate.LocationIds, OriginalLocations = aggregate.Locations,
        Locations = new() { aggregate.Location }, Location = aggregate.Location,
        Geometry = new Point(15, 59), ClusterRadiusMeters = 100
    };

    private static CachedLocationAggregate MakeAggregate(int count)
    {
        var locations = Enumerable.Range(0, count).Select(i => new Location
        {
            Type = SOS.Lib.Enums.LocationType.Point, LocationId = $"site-{i}",
            DecimalLongitude = 15, DecimalLatitude = 59,
            Locality = $"Name {i}", CoordinateUncertaintyInMeters = 10 + i
        }).ToList();
        return new CachedLocationAggregate
        {
            Location = locations[0], Locations = locations, DistinctLocationId = locations[0].DistinctLocationId,
            LocationIds = locations.Select(l => l.LocationId).ToList(),
            ObservationSummary = new LocationObservationSummary { ObservationCount = count }
        };
    }

    private static async Task<JsonElement> Write(LocationFeature feature)
    {
        var context = new DefaultHttpContext();
        using var stream = new MemoryStream();
        context.Response.Body = stream;
        await AdaptiveSearchLocationsGeoJsonWriter.WriteFeatureCollectionAsync(new[] { feature }, null, context.Response);
        using var document = JsonDocument.Parse(stream.ToArray());
        return document.RootElement.GetProperty("features")[0].Clone();
    }
}
