using SOS.Api.Contracts.Observations;
using SOS.Api.Contracts.Areas.Requests;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.DataStewardship.Responses;
using SOS.Api.Contracts.Exports.Responses;
using SOS.Api.Contracts.Lookups.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Api.Contracts.Users.Responses;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Xunit;

namespace SOS.Api.Contracts.UnitTests;

public class EnumJsonSerializationTests
{
    [Theory]
    [InlineData(typeof(TimeRangeDto))]
    [InlineData(typeof(DiffusionStatus))]
    [InlineData(typeof(SightingTypeFilterDto))]
    [InlineData(typeof(AreaTypeDto))]
    [InlineData(typeof(OutputFieldSet))]
    [InlineData(typeof(PropertyFieldDataType))]
    [InlineData(typeof(DsAccessRights))]
    [InlineData(typeof(DsProgrammeArea))]
    [InlineData(typeof(DsPurpose))]
    [InlineData(typeof(DsProjectType))]
    public void Enum_UsesExistingApiNamesWithoutGlobalConverter_AndStillAcceptsNumbers(Type enumType)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var apiOptions = new JsonSerializerOptions(options);
        apiOptions.Converters.Add(new JsonStringEnumConverter());

        foreach (var value in Enum.GetValues(enumType))
        {
            var json = JsonSerializer.Serialize(value, enumType, options);
            using var document = JsonDocument.Parse(json);
            Assert.Equal(JsonValueKind.String, document.RootElement.ValueKind);
            Assert.Equal(value.ToString(), document.RootElement.GetString());
            Assert.Equal(JsonSerializer.Serialize(value, enumType, apiOptions), json);
            Assert.Equal(value, JsonSerializer.Deserialize(json, enumType, options));

            var number = Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
            Assert.Equal(value, JsonSerializer.Deserialize(number, enumType, options));
        }
    }

    [Fact]
    public void SearchFilters_ReadEnumCollectionsAndNestedEnumsWithoutGlobalConverter()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var filter = JsonSerializer.Deserialize<SearchFilterInternalDto>("""
            {
                "date": { "timeRanges": ["Morning", "Night"] },
                "diffusionStatuses": ["NotDiffused", "DiffusedByProvider"],
                "artportalen": { "typeFilter": "ShowBoth" },
                "extendedFilter": { "typeFilter": "ShowOnlyMerged" },
                "geographics": { "areas": [{ "areaType": "County", "featureId": "01" }] },
                "output": { "fieldSet": "Extended" }
            }
            """, options)!;

        Assert.Equal(new[] { TimeRangeDto.Morning, TimeRangeDto.Night }, filter.Date!.TimeRanges);
        Assert.Equal(new[] { DiffusionStatus.NotDiffused, DiffusionStatus.DiffusedByProvider }, filter.DiffusionStatuses);
        Assert.Equal(SightingTypeFilterDto.ShowBoth, filter.Artportalen!.TypeFilter);
        Assert.Equal(SightingTypeFilterDto.ShowOnlyMerged, filter.ExtendedFilter!.TypeFilter);
        Assert.Equal(AreaTypeDto.County, Assert.Single(filter.Geographics!.Areas!).AreaType);
        Assert.Equal(OutputFieldSet.Extended, filter.Output!.FieldSet);
        AssertSameApiJson(filter);

        var signalFilter = JsonSerializer.Deserialize<SignalFilterDto>(
            """{"artportalenTypeFilter":"ShowChildrenAndReplacements"}""", options)!;
        Assert.Equal(SightingTypeFilterDto.ShowChildrenAndReplacements, signalFilter.ArtportalenTypeFilter);
        AssertSameApiJson(signalFilter);
    }

    [Fact]
    public void LookupAndExportModels_ReadEnumsWithoutGlobalConverter()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var field = JsonSerializer.Deserialize<PropertyFieldDescriptionDto>("""
            { "dataType": "DateTime", "fieldSet": "Minimum", "partOfFieldSets": ["Minimum", "Extended"] }
            """, options)!;
        Assert.Equal(PropertyFieldDataType.DateTime, field.DataType);
        Assert.Equal(OutputFieldSet.Minimum, field.FieldSet);
        Assert.Equal(new[] { OutputFieldSet.Minimum, OutputFieldSet.Extended }, field.PartOfFieldSets);
        AssertSameApiJson(field);

        const string areaJson = """{"areaType":"Province"}""";
        var userArea = JsonSerializer.Deserialize<UserAreaDto>(areaJson, options)!;
        var area = JsonSerializer.Deserialize<AreaBaseDto>(areaJson, options)!;
        var areaFilter = JsonSerializer.Deserialize<AreaFilterDto>(areaJson, options)!;
        Assert.Equal(AreaTypeDto.Province, userArea.AreaType);
        Assert.Equal(AreaTypeDto.Province, area.AreaType);
        Assert.Equal(AreaTypeDto.Province, areaFilter.AreaType);
        AssertSameApiJson(userArea);
        AssertSameApiJson(area);
        AssertSameApiJson(areaFilter);

        var job = JsonSerializer.Deserialize<ExportJobInfoDto>("""{"outputFieldSet":"All"}""", options)!;
        Assert.Equal(OutputFieldSet.All, job.OutputFieldSet);
        AssertSameApiJson(job);
        Assert.Null(JsonSerializer.Deserialize<OutputFilterDto>("""{"fieldSet":null}""", options)!.FieldSet);
    }

    [Fact]
    public void DatasetEnums_PreserveNamesAndNullsWithoutGlobalConverter()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var dataset = JsonSerializer.Deserialize<DsDatasetDto>("""
            {
                "programmeArea": "KustOchHav",
                "purpose": "NationellMiljöövervakning",
                "accessRights": "EjOffentlig",
                "projects": [{ "projectType": "GemensamtDelprogram" }]
            }
            """, options)!;
        Assert.Equal(DsProgrammeArea.KustOchHav, dataset.ProgrammeArea);
        Assert.Equal(DsPurpose.NationellMiljöövervakning, dataset.Purpose);
        Assert.Equal(DsAccessRights.EjOffentlig, dataset.AccessRights);
        Assert.Equal(DsProjectType.GemensamtDelprogram, Assert.Single(dataset.Projects!).ProjectType);
        AssertSameApiJson(dataset);

        var emptyDataset = JsonSerializer.Deserialize<DsDatasetDto>("""
            { "programmeArea": null, "purpose": null, "accessRights": null, "projects": [{ "projectType": null }] }
            """, options)!;
        Assert.Null(emptyDataset.ProgrammeArea);
        Assert.Null(emptyDataset.Purpose);
        Assert.Null(emptyDataset.AccessRights);
        Assert.Null(Assert.Single(emptyDataset.Projects!).ProjectType);
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(emptyDataset, options));
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("programmeArea").ValueKind);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("purpose").ValueKind);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("accessRights").ValueKind);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("projects")[0].GetProperty("projectType").ValueKind);
    }

    [Fact]
    public void ObservationLocation_TypeStillUsesNumbersWithoutGlobalConverter()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var location = new Location { Type = LocationType.Point };
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(location, options));
        Assert.Equal(JsonValueKind.Number, document.RootElement.GetProperty("type").ValueKind);
        Assert.Equal((int)LocationType.Point, document.RootElement.GetProperty("type").GetInt32());
    }

    private static void AssertSameApiJson<T>(T model)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var apiOptions = new JsonSerializerOptions(options);
        apiOptions.Converters.Add(new JsonStringEnumConverter());
        Assert.True(JsonNode.DeepEquals(
            JsonSerializer.SerializeToNode(model, options),
            JsonSerializer.SerializeToNode(model, apiOptions)));
    }
}
