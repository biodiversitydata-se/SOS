using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Exports.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Api.Contracts.Taxonomy.Requests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Moq;
using SOS.Observations.Api.Controllers;
using SOS.Observations.Api.Extensions;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using SOS.Lib.Swagger;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Swagger;

public class OpenApiContractTests(OpenApiContractFixture fixture) : IClassFixture<OpenApiContractFixture>
{
    private readonly IReadOnlyDictionary<string, OpenApiDocument> _documents = fixture.Documents;

    [Theory]
    [InlineData("PublicSosObservationsv1.5")]
    [InlineData("InternalSosObservationsv1.5")]
    public void DataStewardshipLocation_AllowsNullAdministrativeAreasWithoutChangingEnumValues(string documentName)
    {
        var document = _documents[documentName];
        var location = document.Components.Schemas["DsLocationDto"];
        foreach (var (property, enumName) in new[]
        {
            ("county", "DsCounty"), ("province", "DsProvince"),
            ("municipality", "Municipality"), ("parish", "DsParish")
        })
        {
            Assert.DoesNotContain(property, location.Required);
            var schema = location.Properties[property];
            Assert.Equal(JsonSchemaType.String | JsonSchemaType.Null, schema.Type);
            Assert.Equal(enumName, Assert.IsType<OpenApiSchemaReference>(schema.AnyOf[0]).Reference.Id);
            Assert.Null(Assert.Single(schema.AnyOf[1].Enum));
            Assert.DoesNotContain(document.Components.Schemas[enumName].Enum, value => value is null);
        }
        Assert.Contains("locationID", location.Required);
        Assert.Contains("emplacement", location.Required);
        using var writer = new StringWriter();
        document.SerializeAsV3(new OpenApiJsonWriter(writer));
        using var json = JsonDocument.Parse(writer.ToString());
        var properties = json.RootElement.GetProperty("components").GetProperty("schemas")
            .GetProperty("DsLocationDto").GetProperty("properties");
        foreach (var property in new[] { "county", "province", "municipality", "parish" })
            Assert.True(properties.GetProperty(property).GetProperty("nullable").GetBoolean());
    }

    [Theory]
    [InlineData("InternalSosObservationsv1.5", "/Observations/Internal/AdaptiveSearch", 2)]
    [InlineData("AzureInternalSosObservationsv1.5", "/Observations/Internal/AdaptiveSearch", 2)]
    [InlineData("InternalSosObservationsv1.5", "/Observations/Internal/AdaptiveSearchLocations", 1)]
    [InlineData("AzureInternalSosObservationsv1.5", "/Observations/Internal/AdaptiveSearchLocations", 1)]
    public void AdaptiveGeoJson_ReferencesSharedFeatureCollectionWithoutWrapperSchemas(string documentName, string path, int representationCount)
    {
        var document = _documents[documentName];
        var response = document.Paths[path].Operations[HttpMethod.Post].Responses["200"];
        Assert.Equal(representationCount, response.Content.Count);
        var reference = Assert.IsType<OpenApiSchemaReference>(response.Content["application/geo+json"].Schema);
        Assert.Equal("FeatureCollection", reference.Reference.Id);
        var collection = document.Components.Schemas["FeatureCollection"];

        Assert.Equal(JsonSchemaType.Object, collection.Type);
        Assert.Equal("FeatureCollection", collection.Properties["type"].Enum.Single()!.GetValue<string>());
        Assert.Equal(new[] { "features", "type" }, collection.Required.Order());
        Assert.Equal(JsonSchemaType.Array, collection.Properties["bbox"].Type);

        var feature = collection.Properties["features"].Items;
        Assert.Equal("Feature", feature.Properties["type"].Enum.Single()!.GetValue<string>());
        Assert.Equal(new[] { "geometry", "properties", "type" }, feature.Required.Order());
        var geometry = feature.Properties["geometry"];
        Assert.Equal(2, geometry.AnyOf.Count);
        Assert.Equal("Geometry", Assert.IsType<OpenApiSchemaReference>(geometry.AnyOf[0]).Reference.Id);
        Assert.Null(Assert.Single(geometry.AnyOf[1].Enum));
        Assert.Equal(JsonSchemaType.Object | JsonSchemaType.Null, feature.Properties["properties"].Type);
        Assert.True(feature.Properties["properties"].AdditionalPropertiesAllowed);
        Assert.DoesNotContain("crs", collection.Properties.Keys);
        Assert.DoesNotContain("crs", feature.Properties.Keys);

        foreach (var name in new[] { "GeoJsonFeatureCollectionDto", "GeoJsonFeatureDto", "GeoJSONObjectType", "ICRSObject" })
            Assert.DoesNotContain(name, document.Components.Schemas.Keys);

        using var writer = new StringWriter();
        document.SerializeAsV3(new OpenApiJsonWriter(writer));
        using var json = JsonDocument.Parse(writer.ToString());
        var responseSchema = json.RootElement.GetProperty("paths").GetProperty(path).GetProperty("post")
            .GetProperty("responses").GetProperty("200").GetProperty("content")
            .GetProperty("application/geo+json").GetProperty("schema");
        Assert.Equal("$ref", Assert.Single(responseSchema.EnumerateObject()).Name);
        Assert.Equal("#/components/schemas/FeatureCollection", responseSchema.GetProperty("$ref").GetString());
    }

    [Theory]
    [InlineData("InternalSosObservationsv1.5")]
    [InlineData("AzureInternalSosObservationsv1.5")]
    public void AdaptiveSearch_DocumentsJsonAlternativesAndResponseHeaders(string documentName)
    {
        var response = _documents[documentName].Paths["/Observations/Internal/AdaptiveSearch"]
            .Operations[HttpMethod.Post].Responses["200"];

        Assert.Equal(new[] { "ObservationAdaptiveSearchResultDto", "ObservationLocationAdaptiveSearchResultByLocationDto" },
            response.Content["application/json"].Schema.OneOf.Select(schema => Assert.IsType<OpenApiSchemaReference>(schema).Reference.Id));
        Assert.Equal(5, response.Headers.Count);
        foreach (var (name, type, format) in new[]
        {
            ("X-Result-Type", JsonSchemaType.String, (string?)null),
            ("X-Observations-TotalCount", JsonSchemaType.Integer, "int64"),
            ("X-Observations-Count", JsonSchemaType.Integer, "int32"),
            ("X-Result-Count", JsonSchemaType.Integer, "int32"),
            ("X-Zoom", JsonSchemaType.Integer, "int32")
        })
        {
            Assert.Equal(type, response.Headers[name].Schema.Type);
            Assert.Equal(format, response.Headers[name].Schema.Format);
        }
    }

    [Theory]
    [InlineData("PropertyFieldDataType")]
    [InlineData("AdaptiveSearchLocationsStrategy")]
    [InlineData("LocationClusteringAlgorithm")]
    [InlineData("ExportFormat")]
    [InlineData("ExportJobStatus")]
    [InlineData("PropertyLabelType")]
    [InlineData("AreaTypeAggregate")]
    [InlineData("OutputFieldSet")]
    [InlineData("DiagramFormat")]
    [InlineData("SearchSortOrder")]
    [InlineData("SpeciesGroup")]
    [InlineData("ApiInformation")]
    [InlineData("TaxonSumAggregationItem")]
    [InlineData("ObservationStatusCounts")]
    [InlineData("TaxonTrendResult")]
    [InlineData("DataQualityReport")]
    [InlineData("DataQualityReportRecord")]
    [InlineData("DataQualityReportObservation")]
    [InlineData("TaxonAreaAggregation")]
    [InlineData("AssociatedMediaType")]
    [InlineData("TaxonRelationsTreeIterationMode")]
    [InlineData("ObjectId")]
    [InlineData("TaxonomyType")]
    [InlineData("AggregationType")]
    [InlineData("CoordinateSys")]
    [InlineData("MetricCoordinateSys")]
    public void PublishedContracts_PreserveComponentNamesWithoutDuplicateDtoSchemas(string name)
    {
        var schemas = _documents["InternalSosObservationsv1.5"].Components.Schemas;
        Assert.Contains(name, schemas.Keys);
        Assert.DoesNotContain($"{name}Dto", schemas.Keys);
    }

    [Fact]
    public void PagedObservationResponse_IncludesOptionalGeoJsonAndMetadata()
    {
        foreach (var documentName in new[] { "InternalSosObservationsv1.5", "AzureInternalSosObservationsv1.5" })
        {
            var properties = _documents[documentName].Components.Schemas["ObservationGeoPagedResultWithMetadataDto"].Properties;
            Assert.Contains("geoJson", properties.Keys);
            Assert.Contains("metadata", properties.Keys);
        }
    }

    [Fact]
    public void SwaggerIgnoreFilter_OnlyRemovesAlwaysIgnoredAndExplicitlyExcludedProperties()
    {
        var schema = new OpenApiSchema
        {
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["alwaysIgnored"] = new OpenApiSchema(),
                ["neverIgnored"] = new OpenApiSchema(),
                ["ignoreWhenNull"] = new OpenApiSchema(),
                ["ignoreWhenDefault"] = new OpenApiSchema(),
                ["excluded"] = new OpenApiSchema()
            }
        };
        var context = new SchemaFilterContext(typeof(IgnoreConditionsModel),
            new Mock<ISchemaGenerator>().Object, new SchemaRepository());

        new SwaggerIgnoreFilter().Apply(schema, context);

        Assert.Equal(new[] { "ignoreWhenDefault", "ignoreWhenNull", "neverIgnored" }, schema.Properties.Keys.Order());
    }

    private sealed class IgnoreConditionsModel
    {
        [JsonIgnore]
        public string? AlwaysIgnored { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? NeverIgnored { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? IgnoreWhenNull { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int IgnoreWhenDefault { get; set; }

        [SwaggerExclude]
        public string? Excluded { get; set; }
    }

    [Fact]
    public void DiffusionStatus_UsesOneSharedContractSchemaForRequestsAndResponses()
    {
        foreach (var document in _documents.Values)
        {
            var schemas = document.Components.Schemas;
            Assert.DoesNotContain("DiffusionStatusDto", schemas.Keys);
            var status = schemas["DiffusionStatus"];
            Assert.Equal(JsonSchemaType.String, status.Type);
            Assert.Equal(new[] { "NotDiffused", "DiffusedBySystem", "DiffusedByProvider" },
                status.Enum.Select(value => value!.GetValue<string>()));
            Assert.Equal("DiffusionStatus", Assert.IsType<OpenApiSchemaReference>(schemas["Observation"].Properties["diffusionStatus"]).Reference.Id);
            Assert.Equal("DiffusionStatus", Assert.IsType<OpenApiSchemaReference>(schemas["SearchFilterBaseDto"].Properties["diffusionStatuses"].Items).Reference.Id);
        }
    }

    [Fact]
    public void ReportAndExportResponses_PreservePublishedSchemaNamesAndProperties()
    {
        var schemas = _documents["InternalSosObservationsv1.5"].Components.Schemas;
        foreach (var name in new[] { "TaxonTrendResult", "DataQualityReport", "DataQualityReportRecord", "DataQualityReportObservation", "File", "TaxonAreaAggregation" })
            Assert.Contains(name, schemas.Keys);
        Assert.DoesNotContain(nameof(ExportFileDto), schemas.Keys);

        Assert.Equal(new[] { "created", "downloadUrl", "name", "size" }, schemas["File"].Properties.Keys.Order());
        Assert.Equal("date-time", schemas["File"].Properties["created"].Format);
        Assert.Equal("int64", schemas["File"].Properties["size"].Format);
        Assert.Equal("DataQualityReportRecord", Assert.IsType<OpenApiSchemaReference>(schemas["DataQualityReport"].Properties["records"].Items).Reference.Id);
        Assert.Equal("DataQualityReportObservation", Assert.IsType<OpenApiSchemaReference>(schemas["DataQualityReportRecord"].Properties["observations"].Items).Reference.Id);
    }

    private static void AssertDescriptionsUseLf(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals("description") && property.Value.ValueKind == JsonValueKind.String)
                    Assert.DoesNotContain('\r', property.Value.GetString()!);
                AssertDescriptionsUseLf(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                AssertDescriptionsUseLf(item);
        }
    }

    [Fact]
    public void AdaptiveLocationParameters_PublishExistingEnumNames()
    {
        Assert.Equal(new[] { 1, 2, 3 }, Enum.GetValues<LocationClusteringAlgorithm>().Select(value => (int)value));
        Assert.Equal(new[] { 1, 2, 3 }, Enum.GetValues<AdaptiveSearchLocationsStrategy>().Select(value => (int)value));

        var document = _documents["InternalSosObservationsv1.5"];
        var parameters = document.Paths["/Observations/Internal/AdaptiveSearchLocations"]
            .Operations[HttpMethod.Post].Parameters;

        Assert.Equal("LocationClusteringAlgorithm", Assert.IsType<OpenApiSchemaReference>(
            parameters.Single(parameter => parameter.Name == "clusteringAlgorithm").Schema).Reference.Id);
        Assert.Equal("AdaptiveSearchLocationsStrategy", Assert.IsType<OpenApiSchemaReference>(
            parameters.Single(parameter => parameter.Name == "adaptiveSearchStrategy").Schema).Reference.Id);
        Assert.Equal(new[] { "Sweref99TmFastGreedy", "Sweref99TmGreedy", "DbScan" },
            document.Components.Schemas["LocationClusteringAlgorithm"].Enum.Select(value => value.GetValue<string>()));
        Assert.Equal(new[] { "ObservationsThenGrid", "ObservationsThenLocationsThenGrid", "LocationsThenGrid" },
            document.Components.Schemas["AdaptiveSearchLocationsStrategy"].Enum.Select(value => value.GetValue<string>()));
    }

    [Fact]
    public void ExportAndDataStewardshipQueryParameters_PreservePublishedSchemaReferences()
    {
        var document = _documents["InternalSosObservationsv1.5"];

        Assert.Contains("OutputFieldSet", document.Components.Schemas.Keys);
        Assert.Contains("CoordinateSys", document.Components.Schemas.Keys);

        foreach (var path in document.Paths.Where(path => path.Key.StartsWith("/Exports/")))
        foreach (var operation in path.Value.Operations.Values)
        foreach (var parameter in operation.Parameters.Where(parameter => parameter.Name == "outputFieldSet"))
            Assert.Equal("OutputFieldSet", Assert.IsType<OpenApiSchemaReference>(parameter.Schema).Reference.Id);

        foreach (var path in document.Paths.Where(path => path.Key.StartsWith("/DataStewardship/")))
        foreach (var operation in path.Value.Operations.Values)
        foreach (var parameter in operation.Parameters.Where(parameter => parameter.Name == "responseCoordinateSystem"))
            Assert.Equal("CoordinateSys", Assert.IsType<OpenApiSchemaReference>(parameter.Schema).Reference.Id);
    }

    [Fact]
    public void RelocatedDtos_PreservePublishedOpenApiComponentNames()
    {
        var schemas = _documents["InternalSosObservationsv1.5"].Components.Schemas;

        foreach (var name in new[]
        {
            "UserInformationDto", "TaxonListDefinitionDto", "SignalFilterDto",
            "ExportFilterDto", "SearchFilterAggregationDto", "GeoGridTileTaxonPageResultDto",
            "DsTaxonDto", "DsDatasetInfoDto", "DsWeatherMeasuringDto",
            "DsEventDto", "DsOccurrenceDto", "DsLocationDto", "PropertyFieldDescriptionDto",
            "LocationDto",
            "ChecklistDto", "ChecklistInternalDto", "DarwinCoreOccurrenceDto",
            "HangfireJobDto", "HarvestInfoDto", "ObservationLocationAdaptiveSearchLocationDto",
            "ObservationLocationAdaptiveSearchResultByLocationDto", "ProjectDto",
            "DataProviderDto", "DataProviderCategory", "VocabularyDto", "VocabularyIdDto",
            "VocabularyValueInfoCategoryDto", "VocabularyValueInfoDto", "VocabularyValueTranslationDto",
            "ExternalSystemIdDto", "ExternalSystemMappingDto", "ExternalSystemMappingFieldDto",
            "ExternalSystemMappingValueDto",
            "ProcessInfoDto", "ProviderInfoDto", "SearchIndexInfoDto", "AllocationInfo",
            "MongoDbInfoDto", "ProcessSummaryDto", "DataProviderStatusDto",
            "SearchFilterDto", "SearchFilterBaseDto", "DsProjectDto", "ExportJobInfoDto",
            "GeoGridResultDto", "TaxonAggregationItemDto", "Multimedia", "MultimediaComment",
            "DiffusionStatus", "ModifiedDateFilterDto", "OccurrenceStatusFilterValuesDto",
            "SightingDeterminationFilterDto", "SightingNotRecoveredFilterDto", "SightingTypeFilterDto",
            "StatusVerificationDto", "TaxonListOperatorDto", "TimeRangeDto"
        })
        {
            Assert.Contains(name, schemas.Keys);

        }

        foreach (var (type, property, reference) in new[]
        {
            ("DsLocationDto", "emplacement", "Geometry"),
            ("DsOccurrenceDto", "observationPoint", "Point"),
            ("PropertyFieldDescriptionDto", "dataType", "PropertyFieldDataType"),
            ("PropertyFieldDescriptionDto", "fieldSet", "OutputFieldSet"),
            ("LocationDto", "point", "Point"),
            ("LocationDto", "pointWithBuffer", "Geometry"),
            ("LocationDto", "pointWithDisturbanceBuffer", "Geometry"),
            ("LocationDto", "type", "LocationType"),
            ("ChecklistDto", "event", "Event"),
            ("ChecklistInternalDto", "event", "Event"),
            ("HangfireJobDto", "id", "ObjectId"),
            ("ObservationLocationAdaptiveSearchLocationDto", "geometry", "Geometry"),
            ("ObservationLocationAdaptiveSearchLocationDto", "statusCounts", "ObservationStatusCounts")
        })
        {
            Assert.Equal(reference, Assert.IsType<OpenApiSchemaReference>(schemas[type].Properties[property]).Reference.Id);
            Assert.Contains(reference, schemas.Keys);
        }
        Assert.Equal("OutputFieldSet", Assert.IsType<OpenApiSchemaReference>(
            schemas["PropertyFieldDescriptionDto"].Properties["partOfFieldSets"].Items).Reference.Id);
        Assert.Equal("ProcessInfoDto", Assert.IsType<OpenApiSchemaReference>(
            schemas["ProcessInfoDto"].Properties["metadataInfo"].Items).Reference.Id);
        Assert.Equal("ProviderInfoDto", Assert.IsType<OpenApiSchemaReference>(
            schemas["ProcessInfoDto"].Properties["providersInfo"].Items).Reference.Id);
        Assert.Equal("AllocationInfo", Assert.IsType<OpenApiSchemaReference>(
            schemas["SearchIndexInfoDto"].Properties["allocations"].Items).Reference.Id);
        Assert.Equal("Multimedia", Assert.IsType<OpenApiSchemaReference>(
            schemas["DarwinCoreOccurrenceDto"].Properties["media"].Items).Reference.Id);
        foreach (var name in new[] { "ArtportalenInternal", "Event", "Occurrence" })
            Assert.Equal("Multimedia", Assert.IsType<OpenApiSchemaReference>(
                schemas[name].Properties["media"].Items).Reference.Id);
    }

    [Fact]
    public void GeneratedDocuments_HaveValidReferencesNamesAndLineEndings()
    {
        foreach (var document in _documents.Values)
        {
            Assert.DoesNotContain(document.Components.Schemas.Keys, name => name.StartsWith("Contract", StringComparison.Ordinal));
            using var writer = new StringWriter();
            document.SerializeAsV3(new OpenApiJsonWriter(writer));
            using var json = JsonDocument.Parse(writer.ToString());
            AssertDescriptionsUseLf(json.RootElement);
            foreach (System.Text.RegularExpressions.Match reference in System.Text.RegularExpressions.Regex.Matches(
                writer.ToString(), "#/components/schemas/([^\"/]+)"))
            {
                Assert.Contains(reference.Groups[1].Value, document.Components.Schemas.Keys);
            }
        }
    }

    [Fact]
    public void SharedEnums_PreservePublishedOpenApiComponentNames()
    {
        var schemas = _documents["InternalSosObservationsv1.5"].Components.Schemas;

        Assert.Contains("OutputFieldSet", schemas.Keys);

        Assert.Contains("CreatePolygonsMode", schemas.Keys);
        Assert.Contains("MetricCoordinateSys", schemas.Keys);

        foreach (var name in new[]
        {
            "PublicSosObservationsv1.5", "InternalSosObservationsv1.5",
            "AzurePublicSosObservationsv1.5", "AzureInternalSosObservationsv1.5"
        })
        {
            var document = _documents[name];
            Assert.Contains("SearchSortOrder", document.Components.Schemas.Keys);

            if (!document.Paths.TryGetValue("/DOIs", out var doiPath)) continue;
            var sortOrder = doiPath.Operations[HttpMethod.Get].Parameters.Single(parameter => parameter.Name == "sortOrder");
            Assert.Equal("SearchSortOrder", Assert.IsType<OpenApiSchemaReference>(sortOrder.Schema).Reference.Id);
        }
    }

    [Fact]
    public void FormerContractDuplicates_UsePublishedOpenApiNames()
    {
        var names = new[]
        {
            "GeographicsFilterDto", "LatLonBoundingBoxDto", "AreaTypeDto", "Area",
            "LocationAttributes", "OutputFieldSet", "TaxonomyType", "TaxonFilterDto",
            "LocationType", "ArtportalenFilterDto", "AreaWithGeometryDto"
        };

        foreach (var documentName in new[]
        {
            "PublicSosObservationsv1.5", "InternalSosObservationsv1.5",
            "AzurePublicSosObservationsv1.5", "AzureInternalSosObservationsv1.5"
        })
        {
            var schemas = _documents[documentName].Components.Schemas;
            Assert.DoesNotContain("IGeometryObject", schemas.Keys);
            Assert.DoesNotContain("GeoJSONObjectType", schemas.Keys);
            var geometry = Assert.IsType<OpenApiSchema>(schemas["Geometry"]);
            Assert.True(geometry.AdditionalPropertiesAllowed);
            Assert.Equal(7, geometry.OneOf.Count);
            Assert.Equal("Geometry", Assert.IsType<OpenApiSchemaReference>(
                geometry.OneOf.Last().Properties["geometries"].Items).Reference.Id);

            var locationType = Assert.IsType<OpenApiSchema>(schemas["Location"].Properties["type"]);
            Assert.Equal(JsonSchemaType.Integer, locationType.Type);
            Assert.Equal("Location type. 1=Point, 2=Polygon.", locationType.Description);

            foreach (var name in names.Except(["ArtportalenFilterDto", "AreaWithGeometryDto"]))
                Assert.Contains(name, schemas.Keys);
            if (!documentName.StartsWith("AzurePublic", StringComparison.Ordinal))
                Assert.Contains("ArtportalenFilterDto", schemas.Keys);
            if (documentName.Contains("Internal", StringComparison.Ordinal))
            {
                Assert.Contains("AreaWithGeometryDto", schemas.Keys);
            }
            Assert.DoesNotContain("LatLonGeoLocation", schemas.Keys);

            Assert.Equal("Geometry", Assert.IsType<OpenApiSchemaReference>(
                schemas["GeographicsFilterDto"].Properties["geometries"].Items).Reference.Id);
            Assert.Equal("TaxonomyType", Assert.IsType<OpenApiSchemaReference>(
                schemas["TaxonFilterDto"].Properties["taxonomyType"]).Reference.Id);
        }
    }

    [Fact]
    public void PagedResponses_PreservePublishedComponentNames()
    {
        var schemas = _documents["InternalSosObservationsv1.5"].Components.Schemas;

        Assert.Contains("AreaBaseDtoPagedResult", schemas.Keys);
        Assert.Contains("ObservationGeoPagedResultWithMetadataDto", schemas.Keys);
    }

    [Fact]
    public void ObservationResponsesAndChecklists_UseOnePublishedWeatherSchema()
    {
        var schemas = _documents["InternalSosObservationsv1.5"].Components.Schemas;

        foreach (var resultName in new[] { "ObservationGeoPagedResultWithMetadataDto", "ObservationScrollResultDto" })
            Assert.Equal("Observation", Assert.IsType<OpenApiSchemaReference>(
                Assert.IsType<OpenApiSchema>(schemas[resultName].Properties["records"]).Items).Reference.Id);

        Assert.Equal("Observation", Assert.IsType<OpenApiSchemaReference>(
            Assert.IsType<OpenApiSchema>(schemas["ObservationAdaptiveSearchResultDto"].Properties["observationsResult"]).Items).Reference.Id);
        foreach (var checklistName in new[] { "ChecklistDto", "ChecklistInternalDto" })
            Assert.Equal("Event", Assert.IsType<OpenApiSchemaReference>(
                schemas[checklistName].Properties["event"]).Reference.Id);
        Assert.Equal("Weather", Assert.IsType<OpenApiSchemaReference>(
            schemas["Event"].Properties["weather"]).Reference.Id);

    }

    [Fact]
    public void VocabularyValue_UsesOnePublishedSchemaInEveryDocument()
    {
        foreach (var name in new[] { "PublicSosObservationsv1.5", "InternalSosObservationsv1.5", "AzurePublicSosObservationsv1.5", "AzureInternalSosObservationsv1.5" })
        {
            var schemas = _documents[name].Components.Schemas;
            Assert.Contains("VocabularyValue", schemas.Keys);

            Assert.Equal(new[] { "id", "value" }, schemas["VocabularyValue"].Properties.Keys);
            Assert.Equal("VocabularyValue", Assert.IsType<OpenApiSchemaReference>(
                schemas["Observation"].Properties["accessRights"]).Reference.Id);
            if (schemas.TryGetValue("Location", out var location))
                Assert.Equal("VocabularyValue", Assert.IsType<OpenApiSchemaReference>(
                    location.Properties["continent"]).Reference.Id);
        }
    }

    [Fact]
    public void AreaTypeDto_UsesSinglePublishedOpenApiSchema()
    {
        var document = _documents["InternalSosObservationsv1.5"];
        var schemas = document.Components.Schemas;

        Assert.Contains("AreaTypeDto", schemas.Keys);

        Assert.Equal("AreaTypeDto", Assert.IsType<OpenApiSchemaReference>(
            schemas["AreaWithGeometryDto"].Properties["areaType"]).Reference.Id);
        Assert.Equal("AreaTypeDto", Assert.IsType<OpenApiSchemaReference>(
            schemas["AreaBaseDto"].Properties["areaType"]).Reference.Id);

        var areaTypesParameter = document.Paths["/Areas"].Operations[HttpMethod.Get].Parameters
            .Single(parameter => parameter.Name == "areaTypes");
        var arraySchema = Assert.IsType<OpenApiSchema>(areaTypesParameter.Schema);
        Assert.Equal("AreaTypeDto", Assert.IsType<OpenApiSchemaReference>(arraySchema.Items).Reference.Id);
    }

    [Fact]
    public void BoundingBoxes_UseOnlySharedDtoSchemas()
    {
        var schemas = _documents["InternalSosObservationsv1.5"].Components.Schemas;

        foreach (var schemaName in new[]
        {
            "AreaBaseDto", "AreaWithGeometryDto", "GeographicsFilterDto", "GeoGridCellDto",
            "GeoGridMetricResultDto", "GeoGridResultDto", "GeoGridTileTaxaCellDto", "GridCellDto",
            "ObservationAdaptiveSearchResultDto", "ObservationLocationAdaptiveSearchResultByLocationDto"
        })
        {
            Assert.Equal("LatLonBoundingBoxDto", Assert.IsType<OpenApiSchemaReference>(
                schemas[schemaName].Properties["boundingBox"]).Reference.Id);
        }

        var boundingBox = schemas["LatLonBoundingBoxDto"];
        foreach (var corner in new[] { "topLeft", "bottomRight" })
        {
            Assert.Equal("LatLonCoordinateDto", Assert.IsType<OpenApiSchemaReference>(
                boundingBox.Properties[corner]).Reference.Id);
        }

        Assert.Contains("LatLonCoordinateDto", schemas.Keys);
        Assert.DoesNotContain("LatLonBoundingBox", schemas.Keys);
        Assert.DoesNotContain("LatLonCoordinate", schemas.Keys);
    }

    [Fact]
    public void StatusIdentifier_DoesNotMarkItsFieldsReadOnly()
    {
        var schemas = _documents["InternalSosObservationsv1.5"].Components.Schemas;

        Assert.False(schemas["ObjectId"].Properties["timestamp"].ReadOnly);
        Assert.False(schemas["ObjectId"].Properties["creationTime"].ReadOnly);
    }

    [Fact]
    public void SpeciesGroup_PreservesPublishedStringEnumAndSchemaName()
    {
        var expectedNames = new[]
        {
            "All", "VascularPlants", "Mosses", "Lichens", "Fungi", "AlgaeAndMicroOrganisms",
            "Invertebrates", "Birds", "AmphibiansAndReptiles", "OtherVertebrates", "Bats", "Fishes"
        };
        Assert.Contains("SpeciesGroup", _documents["InternalSosObservationsv1.5"].Components.Schemas.Keys);

        foreach (var document in _documents.Values)
        {
            var schemas = document.Components.Schemas;
            Assert.DoesNotContain($"{nameof(SpeciesGroup)}Dto", schemas.Keys);
            if (!schemas.TryGetValue("TaxonAttributes", out var attributes)) continue;

            Assert.Equal("SpeciesGroup", Assert.IsType<OpenApiSchemaReference>(
                attributes.Properties["speciesGroup"]).Reference.Id);
            Assert.Equal(JsonSchemaType.String, schemas["SpeciesGroup"].Type);
            Assert.Equal(expectedNames, schemas["SpeciesGroup"].Enum.Select(value => value!.GetValue<string>()));
        }
    }

    [Fact]
    public void TaxonDiagram_PublishesExistingEnumNamesAndQuerySchemas()
    {
        Assert.Equal(new[] { 0, 1 }, Enum.GetValues<TaxonomyType>().Select(value => (int)value));
        Assert.Equal(new[] { 0, 1 }, Enum.GetValues<DiagramFormat>().Select(value => (int)value));
        Assert.Equal(new[] { 0, 1, 2 }, Enum.GetValues<TaxonRelationsTreeIterationMode>().Select(value => (int)value));

        var document = _documents["InternalSosObservationsv1.5"];
        var parameters = document.Paths["/Systems/TaxonRelationsDiagram"].Operations[HttpMethod.Get].Parameters;
        Assert.Equal(new[] { "GraphViz", "Mermaid" }, document.Components.Schemas["DiagramFormat"].Enum.Select(value => value!.GetValue<string>()));
        Assert.Equal(new[] { "BothParentsAndChildren", "OnlyParents", "OnlyChildren" },
            document.Components.Schemas["TaxonRelationsTreeIterationMode"].Enum.Select(value => value!.GetValue<string>()));
        foreach (var (name, schemaName) in new[]
        {
            ("treeIterationMode", "TaxonRelationsTreeIterationMode"),
            ("diagramFormat", "DiagramFormat"),
            ("taxonomyType", "TaxonomyType")
        })
            Assert.Equal(schemaName, Assert.IsType<OpenApiSchemaReference>(parameters.Single(p => p.Name == name).Schema).Reference.Id);
    }

    [Fact]
    public void LegacyExportAndAggregationFilters_PreservePublishedReferences()
    {
        var schemas = _documents["PublicSosObservationsv1.5"].Components.Schemas;

        foreach (var name in new[] { "ExportFilterDto", "SearchFilterAggregationDto" })
        {
            var properties = schemas[name].Properties;
            Assert.Equal("DateFilterDto", Assert.IsType<OpenApiSchemaReference>(properties["date"]).Reference.Id);
            Assert.Equal("GeographicsFilterDto", Assert.IsType<OpenApiSchemaReference>(properties["geographics"]).Reference.Id);
            Assert.Equal("TaxonFilterDto", Assert.IsType<OpenApiSchemaReference>(properties["taxon"]).Reference.Id);
        }
    }

    [Fact]
    public void MetricGridAggregation_PublishesMetricCoordinateSystemParameter()
    {
        var document = _documents["InternalSosObservationsv1.5"];
        var parameter = document.Paths["/Observations/Internal/MetricGridAggregation"].Operations[HttpMethod.Post]
            .Parameters.Single(parameter => parameter.Name == "metricCoordinateSys");
        Assert.Equal("MetricCoordinateSys", Assert.IsType<OpenApiSchemaReference>(parameter.Schema).Reference.Id);
    }

}

public sealed class OpenApiContractFixture
{
    public IReadOnlyDictionary<string, OpenApiDocument> Documents { get; }

    public OpenApiContractFixture()
    {
        using var provider = CreateSwaggerProvider();
        var swagger = provider.GetRequiredService<ISwaggerProvider>();
        Documents = new[]
        {
            "PublicSosObservationsv1.5", "InternalSosObservationsv1.5",
            "AzurePublicSosObservationsv1.5", "AzureInternalSosObservationsv1.5"
        }.ToDictionary(name => name, name => swagger.GetSwagger(name));
    }

    private static ServiceProvider CreateSwaggerProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(x => x.ApplicationName).Returns(typeof(ObservationsController).Assembly.GetName().Name!);
        environment.SetupGet(x => x.EnvironmentName).Returns("Development");
        environment.SetupGet(x => x.ContentRootPath).Returns(AppContext.BaseDirectory);
        environment.SetupGet(x => x.ContentRootFileProvider).Returns(new NullFileProvider());
        environment.SetupGet(x => x.WebRootPath).Returns(AppContext.BaseDirectory);
        environment.SetupGet(x => x.WebRootFileProvider).Returns(new NullFileProvider());
        services.AddSingleton(environment.Object);
        services.AddSingleton<IHostEnvironment>(environment.Object);
        services.AddControllers().AddApplicationPart(typeof(ObservationsController).Assembly)
            .AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.SetupSwagger();
        return services.BuildServiceProvider();
    }
}
