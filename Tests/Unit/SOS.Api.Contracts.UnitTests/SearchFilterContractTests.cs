using FluentAssertions;
using FluentAssertions.Execution;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Aggregations.Responses;
using SOS.Api.Contracts.Analysis.Responses;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.DataStewardship.Responses;
using SOS.Api.Contracts.Exports.Responses;
using SOS.Api.Contracts.Metadata.Responses;
using SOS.Api.Contracts.Observations.Requests;
using System.Text.Json;
using System.Reflection;
using Xunit;

namespace SOS.Api.Contracts.UnitTests;

public class SearchFilterContractTests
{
    private static readonly HashSet<string> IntentionalWireTypeDifferences =
    [
        // The public client preserves offsets even though the current in-process
        // server DTOs use DateTime. Both are ISO-8601 JSON strings on the wire.
        $"{nameof(DateFilterDto)}.{nameof(DateFilterDto.StartDate)}",
        $"{nameof(DateFilterDto)}.{nameof(DateFilterDto.EndDate)}",
        $"{nameof(DateFilterDto)}.{nameof(DateFilterDto.DateFilterType)}",
        $"{nameof(ExtendedFilterDto)}.{nameof(ExtendedFilterDto.ReportedDateFrom)}",
        $"{nameof(ExtendedFilterDto)}.{nameof(ExtendedFilterDto.ReportedDateTo)}",
        $"{nameof(ModifiedDateFilterDto)}.{nameof(ModifiedDateFilterDto.From)}",
        $"{nameof(ModifiedDateFilterDto)}.{nameof(ModifiedDateFilterDto.To)}",
        $"{nameof(SOS.Api.Contracts.Observations.Responses.Observation)}.{nameof(SOS.Api.Contracts.Observations.Responses.Observation.Modified)}",
        $"{nameof(SOS.Api.Contracts.Observations.Responses.Observation)}.{nameof(SOS.Api.Contracts.Observations.Responses.Observation.Created)}",
        $"{nameof(SOS.Api.Contracts.Observations.Responses.Event)}.{nameof(SOS.Api.Contracts.Observations.Responses.Event.StartDate)}",
        $"{nameof(SOS.Api.Contracts.Observations.Responses.Event)}.{nameof(SOS.Api.Contracts.Observations.Responses.Event.EndDate)}",
        $"{nameof(SOS.Api.Contracts.Observations.Responses.Occurrence)}.{nameof(SOS.Api.Contracts.Observations.Responses.Occurrence.ReportedDate)}",

        // GeoJSON.Text exposes its polymorphic geometry contract as an
        // interface; the API's Newtonsoft model uses the concrete base type.
        $"{nameof(GeographicsFilterDto)}.{nameof(GeographicsFilterDto.Geometries)}",
        $"{nameof(AreaWithGeometryDto)}.{nameof(AreaWithGeometryDto.Geometry)}",

        // The API serializes the enum as text. A string keeps EnumMember values
        // forward compatible for consumers of the separately versioned package.
        $"{nameof(DsDatasetDto)}.{nameof(DsDatasetDto.ProgrammeArea)}",
        $"{nameof(DsDatasetDto)}.{nameof(DsDatasetDto.Purpose)}",
        $"{nameof(DsDatasetDto)}.{nameof(DsDatasetDto.AccessRights)}",
        $"{nameof(DsProjectDto)}.{nameof(DsProjectDto.ProjectType)}"
    ];

    [Fact]
    public void PublicSearchFilter_MatchesServerFilterProperties()
    {
        AssertPropertiesMatch(
            typeof(SearchFilterDto),
            typeof(SOS.Lib.Dtos.Filter.SearchFilterDto));
    }

    [Fact]
    public void InternalSearchFilters_MatchServerFilterProperties()
    {
        AssertPropertiesMatch(
            typeof(SearchFilterInternalBaseDto),
            typeof(SOS.Lib.Dtos.Filter.SearchFilterInternalBaseDto));
        AssertPropertiesMatch(
            typeof(SearchFilterInternalDto),
            typeof(SOS.Lib.Dtos.Filter.SearchFilterInternalDto));
        AssertPropertiesMatch(
            typeof(SearchFilterAggregationInternalDto),
            typeof(SOS.Lib.Dtos.Filter.SearchFilterAggregationInternalDto));
    }

    [Fact]
    public void NestedWorkspaceAndObservationContracts_MatchServerProperties()
    {
        using var assertionScope = new AssertionScope();

        AssertPropertiesMatch(typeof(DateFilterDto), typeof(SOS.Lib.Dtos.Filter.DateFilterDto));
        AssertPropertiesMatch(typeof(ExtendedFilterDto), typeof(SOS.Lib.Dtos.Filter.ExtendedFilterDto));
        AssertPropertiesMatch(typeof(GeographicsFilterDto), typeof(SOS.Lib.Dtos.Filter.GeographicsFilterDto));
        AssertPropertiesMatch(typeof(TaxonFilterDto), typeof(SOS.Lib.Dtos.Filter.TaxonFilterDto));
        AssertPropertiesMatch(
            typeof(SOS.Api.Contracts.Observations.Responses.ArtportalenInternal),
            typeof(SOS.Lib.Models.Processed.Observation.ArtportalenInternal));
        AssertPropertiesMatch(
            typeof(SOS.Api.Contracts.Observations.Responses.Observation),
            typeof(SOS.Lib.Models.Processed.Observation.Observation),
            ["MongoDbId"]);
        AssertPropertiesMatch(
            typeof(SOS.Api.Contracts.Observations.Responses.Event),
            typeof(SOS.Lib.Models.Processed.Observation.Event));
        AssertPropertiesMatch(
            typeof(SOS.Api.Contracts.Observations.Responses.Identification),
            typeof(SOS.Lib.Models.Processed.Observation.Identification));
        AssertPropertiesMatch(
            typeof(SOS.Api.Contracts.Observations.Responses.Occurrence),
            typeof(SOS.Lib.Models.Processed.Observation.Occurrence));
        AssertPropertiesMatch(
            typeof(SOS.Api.Contracts.Observations.Responses.Location),
            typeof(SOS.Lib.Models.Processed.Observation.Location));
        AssertPropertiesMatch(
            typeof(SOS.Api.Contracts.Observations.Responses.Taxon),
            typeof(SOS.Lib.Models.Processed.Observation.Taxon));
        AssertPropertiesMatch(
            typeof(SOS.Api.Contracts.Observations.Responses.TaxonAttributes),
            typeof(SOS.Lib.Models.Processed.Observation.TaxonAttributes),
            ["CountyOccurrences"]);
        AssertPropertiesMatch(
            typeof(SOS.Api.Contracts.Observations.Responses.Substrate),
            typeof(SOS.Lib.Models.Processed.Observation.Substrate));
        AssertPropertiesMatch(
            typeof(SOS.Api.Contracts.Observations.Responses.Multimedia),
            typeof(SOS.Lib.Models.Processed.Observation.Multimedia));        
        AssertPropertiesMatch(
            typeof(SOS.Api.Contracts.Observations.Responses.TaxonScientificName),
            typeof(SOS.Lib.Models.Processed.Observation.TaxonScientificName));
    }

    [Fact]
    public void AnalysisExportDataStewardshipAndApiInfoContracts_MatchServerProperties()
    {
        using var assertionScope = new AssertionScope();

        AssertPropertiesMatch(typeof(UserAggregationResponseDto), typeof(SOS.Lib.Dtos.Search.UserAggregationResponseDto));
        AssertPropertiesMatch(typeof(PagedAggregationResultDto<UserAggregationResponseDto>), typeof(SOS.Lib.Dtos.Search.PagedAggregationResultDto<SOS.Lib.Dtos.Search.UserAggregationResponseDto>));
        AssertPropertiesMatch(typeof(ExportJobInfoDto), typeof(SOS.Lib.Dtos.Export.ExportJobInfoDto));
        AssertPropertiesMatch(typeof(DsDatasetDto), typeof(SOS.Lib.Dtos.DataStewardship.DsDatasetDto));
        AssertPropertiesMatch(typeof(DsProjectDto), typeof(SOS.Lib.Dtos.DataStewardship.DsProjectDto));
        AssertPropertiesMatch(typeof(DsOrganisationDto), typeof(SOS.Lib.Dtos.DataStewardship.DsOrganisationDto));
        AssertPropertiesMatch(typeof(DsMethodologyDto), typeof(SOS.Lib.Dtos.DataStewardship.DsMethodologyDto));
        AssertPropertiesMatch(typeof(ApiInformationDto), typeof(SOS.Lib.Models.ApiInfo.ApiInformation));
    }

    [Fact]
    public void AdaptiveSearchAreaAndTaxonAreaContracts_MatchServerProperties()
    {
        using var assertionScope = new AssertionScope();

        AssertPropertiesMatch(
            typeof(TaxonAreaAggregationDto),
            typeof(SOS.Lib.Models.Search.Result.TaxonAreaAggregation));
        AssertPropertiesMatch(
            typeof(AreaWithGeometryDto),
            typeof(SOS.Lib.Dtos.AreaWithGeometryDto));
        AssertPropertiesMatch(
            typeof(SOS.Api.Contracts.Observations.Responses.AdaptiveSearchResultDto<SOS.Api.Contracts.Observations.Responses.Observation>),
            typeof(SOS.Lib.Dtos.AdaptiveSearchResultDto<SOS.Lib.Models.Processed.Observation.Observation>));
        AssertEnumValuesMatch(
            typeof(SOS.Api.Contracts.Areas.Responses.AreaTypeDto),
            typeof(SOS.Lib.Dtos.Enum.AreaTypeDto));
    }

    [Fact]
    public void ObservationDateFilters_UseDateTimeOffsetWireTypes()
    {
        typeof(DateFilterDto).GetProperty(nameof(DateFilterDto.StartDate))!.PropertyType
            .Should().Be(typeof(DateTimeOffset?));
        typeof(DateFilterDto).GetProperty(nameof(DateFilterDto.EndDate))!.PropertyType
            .Should().Be(typeof(DateTimeOffset?));
        typeof(ExtendedFilterDto).GetProperty(nameof(ExtendedFilterDto.ReportedDateFrom))!.PropertyType
            .Should().Be(typeof(DateTimeOffset?));
        typeof(ExtendedFilterDto).GetProperty(nameof(ExtendedFilterDto.ReportedDateTo))!.PropertyType
            .Should().Be(typeof(DateTimeOffset?));
        typeof(ModifiedDateFilterDto).GetProperty(nameof(ModifiedDateFilterDto.From))!.PropertyType
            .Should().Be(typeof(DateTimeOffset?));
        typeof(ModifiedDateFilterDto).GetProperty(nameof(ModifiedDateFilterDto.To))!.PropertyType
            .Should().Be(typeof(DateTimeOffset?));
    }

    [Fact]
    public void DateFilter_PreservesExplicitOffsetsAndMidnight()
    {
        var json = JsonSerializer.Serialize(new DateFilterDto
        {
            StartDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(1)),
            EndDate = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.FromHours(2))
        });

        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("StartDate").GetString()
            .Should().Be("2026-01-01T00:00:00+01:00");
        document.RootElement.GetProperty("EndDate").GetString()
            .Should().Be("2026-07-01T00:00:00+02:00");
    }

    private static void AssertPropertiesMatch(
        Type contractType,
        Type serverType,
        IReadOnlyCollection<string>? ignoredServerProperties = null)
    {
        static bool IsApiProperty(System.Reflection.PropertyInfo property) =>
            property.GetCustomAttributes(true).All(attribute =>
                attribute is not ObsoleteAttribute &&
                attribute is not System.Text.Json.Serialization.JsonIgnoreAttribute &&
                attribute.GetType().Name != "SwaggerExcludeAttribute");

        var contractProperties = contractType.GetProperties()
            .Where(IsApiProperty)
            .Select(property => property.Name)
            .Order()
            .ToArray();
        var serverProperties = serverType.GetProperties()
            .Where(IsApiProperty)
            .Select(property => property.Name)
            .Where(name => ignoredServerProperties is null || !ignoredServerProperties.Contains(name))
            .Order()
            .ToArray();

        contractProperties.Except(serverProperties).Should().BeEmpty(
            $"{contractType.Name} contains properties that do not exist on the server contract");
        serverProperties.Except(contractProperties).Should().BeEmpty(
            $"{contractType.Name} must expose the complete server shape");

        foreach (var contractProperty in contractType.GetProperties().Where(IsApiProperty))
        {
            var serverProperty = serverType.GetProperty(contractProperty.Name);
            if (serverProperty is null || !IsApiProperty(serverProperty))
            {
                continue;
            }

            if (IntentionalWireTypeDifferences.Contains($"{contractType.Name}.{contractProperty.Name}"))
            {
                continue;
            }

            GetWireTypeSignature(contractProperty.PropertyType).Should().Be(
                GetWireTypeSignature(serverProperty.PropertyType),
                $"{contractType.Name}.{contractProperty.Name} must use the same JSON wire type as the server contract");
        }
    }

    private static string GetWireTypeSignature(Type type)
    {
        var nullableType = Nullable.GetUnderlyingType(type);
        if (nullableType is not null)
        {
            return $"nullable<{GetWireTypeSignature(nullableType)}>";
        }

        if (type != typeof(string))
        {
            var enumerableType = type.IsArray
                ? typeof(IEnumerable<>).MakeGenericType(type.GetElementType()!)
                : type.GetInterfaces()
                    .Append(type)
                    .FirstOrDefault(candidate =>
                        candidate.IsGenericType &&
                        candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (enumerableType is not null)
            {
                return $"array<{GetWireTypeSignature(enumerableType.GetGenericArguments()[0])}>";
            }
        }

        if (type.IsEnum)
        {
            return $"enum<{string.Join('|', Enum.GetNames(type).Order())}>";
        }

        if (type.IsPrimitive ||
            type == typeof(string) ||
            type == typeof(decimal) ||
            type == typeof(DateTime) ||
            type == typeof(DateTimeOffset) ||
            type == typeof(Guid) ||
            type == typeof(TimeSpan) ||
            type == typeof(object))
        {
            return type.FullName!;
        }

        return $"object<{type.Name}>";
    }

    private static void AssertEnumValuesMatch(Type contractType, Type serverType)
    {
        var contractValues = Enum.GetNames(contractType)
            .ToDictionary(name => name, name => Convert.ToInt64(Enum.Parse(contractType, name)));
        var serverValues = Enum.GetNames(serverType)
            .ToDictionary(name => name, name => Convert.ToInt64(Enum.Parse(serverType, name)));

        contractValues.Should().Equal(
            serverValues,
            $"{contractType.Name} must use the same names and numeric values as the server contract");
    }
}
