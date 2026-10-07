using Microsoft.Extensions.Caching.Memory;
using Moq;
using GeoJSON.Text.Geometry;
using NetTopologySuite.IO;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Search.Input;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Shared.Api.Configuration;
using SOS.Shared.Api.Validators;
using SOS.Shared.Api.Validators.Interfaces;
using System.Text.Json;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Validators;

public class GeometryValidationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ValidateGeographicalAreaExists_AcceptsGeometryWithoutAreasOrBoundingBox(bool serverInput)
    {
        var source = new GeographicsFilterDto
        {
            Geometries = [new Point(new Position(62, 16))]
        };
        GeographicsFilterDto filter = serverInput ? GeographicsFilterInputModel.FromDto(source) : source;

        Assert.True(CreateValidator().ValidateGeographicalAreaExists(filter).IsSuccess);
        if (serverInput) Assert.Null(filter.Geometries);
    }

    [Fact]
    public void ValidateGeographicalAreaExists_RejectsMissingGeographicCriteria()
    {
        var result = CreateValidator().ValidateGeographicalAreaExists(new GeographicsFilterDto());

        Assert.True(result.IsFailure);
        Assert.Equal("You must provide area/s, geometry or bounding box", result.Error);
    }

    [Theory]
    [InlineData("POINT (18 59)", true)]
    [InlineData("POLYGON ((16 60, 19 60, 19 63, 16 63, 16 60))", true)]
    [InlineData("POLYGON ((16 60, 19 63, 19 60, 16 63, 16 60))", false)]
    public void ValidateGeometries_ValidatesServerGeometriesWhenDtoCollectionIsNull(string wkt, bool isValid)
    {
        var geometry = new WKTReader().Read(wkt);
        var input = new GeographicsFilterInputModel { ServerGeometries = [geometry] };
        GeographicsFilterDto filter = input;
        Assert.Null(filter.Geometries);

        var result = CreateValidator().ValidateGeometries(filter);

        Assert.Equal(isValid, result.IsSuccess);
        if (!isValid) Assert.Equal("Invalid geometry", result.Error);
        Assert.Same(geometry, Assert.Single(input.ServerGeometries!));
        Assert.Null(filter.Geometries);
    }

    [Theory]
    [InlineData("""{"type":"Point","coordinates":[18,59]}""", true)]
    [InlineData("""{"type":"Polygon","coordinates":[[[16,60],[19,60],[19,63],[16,63],[16,60]]]}""", true)]
    [InlineData("""{"type":"Polygon","coordinates":[[[16,60],[19,63],[19,60],[16,63],[16,60]]]}""", false)]
    public void ValidateGeometries_ValidatesProgrammaticallyCreatedGeoJsonFilter(string json, bool isValid)
    {
        var filter = new GeographicsFilterDto
        {
            Geometries = [JsonSerializer.Deserialize<IGeometryObject>(json)!]
        };

        var result = CreateValidator().ValidateGeometries(filter);

        Assert.Equal(isValid, result.IsSuccess);
        if (!isValid) Assert.Equal("Invalid geometry", result.Error);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Validation_RejectsConversionFailureEvenWhenOtherGeometriesAreValid(bool serverInput)
    {
        var source = new GeographicsFilterDto
        {
            Geometries =
            [
                new Point(new Position(62, 16)),
                JsonSerializer.Deserialize<IGeometryObject>(
                    """{"type":"Polygon","coordinates":[[[16,62],[17,63]]]}""")!
            ]
        };
        GeographicsFilterDto filter = serverInput ? GeographicsFilterInputModel.FromDto(source) : source;
        var validator = CreateValidator();

        var geometryResult = validator.ValidateGeometries(filter);
        var filterResult = await validator.ValidateSearchFilterAsync(
            new SearchFilterDto { Geographics = filter }, bboxMandatory: true);

        Assert.True(geometryResult.IsFailure);
        Assert.Equal("Invalid geometry", geometryResult.Error);
        Assert.True(filterResult.IsFailure);
        Assert.Equal("Invalid geometry", filterResult.Error);
        Assert.True(validator.ValidateGeographicalAreaExists(filter).IsSuccess);
    }

    [Fact]
    public void ValidateGeometries_AcceptsMissingFilter()
    {
        Assert.True(CreateValidator().ValidateGeometries(null).IsSuccess);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ValidateGeometries_AcceptsMissingOrEmptyGeometryCollection(bool serverInput, bool emptyCollection)
    {
        GeographicsFilterDto filter = serverInput
            ? new GeographicsFilterInputModel { ServerGeometries = emptyCollection ? [] : null }
            : new GeographicsFilterDto { Geometries = emptyCollection ? [] : null };

        Assert.True(CreateValidator().ValidateGeometries(filter).IsSuccess);
    }

    private static IInputValidator CreateValidator() => new InputValidator(
        Mock.Of<IAreaCache>(),
        new SortableFieldsCache(Mock.Of<IProcessedObservationCoreRepository>(), Mock.Of<IMemoryCache>()),
        Mock.Of<ITaxonManager>(),
        new InputValaidationConfiguration
        {
            CountFactor = 1,
            ElasticSearchMaxRecordsInternal = 1000,
            ElasticSearchMaxRecordsPublic = 1000,
            MaxBatchSize = 100,
            MaxNrElasticSearchAggregationBuckets = 1000,
            SignalSearchTaxonListIds = [1],
            TilesLimitInternal = 1000,
            TilesLimitPublic = 1000
        });
}
