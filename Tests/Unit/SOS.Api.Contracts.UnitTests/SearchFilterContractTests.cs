
using FluentAssertions;
using FluentAssertions.Execution;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Aggregations.Responses;
using SOS.Api.Contracts.Analysis.Responses;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.DataStewardship.Responses;
using SOS.Api.Contracts.Exports.Responses;
using SOS.Api.Contracts.Observations.Requests;
using System.Text.Json;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Xunit;
using SOS.Api.Contracts.Metadata.Responses;

namespace SOS.Api.Contracts.UnitTests;

public class SearchFilterContractTests
{
    [Fact]
    public void SharedFilterEnums_PreservePublishedValues()
    {
        Assert.Equal(new[] { "Minimum", "Extended", "AllWithValues", "All", "None" }, Enum.GetNames<OutputFieldSet>());
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, Enum.GetValues<OutputFieldSet>().Select(value => (int)value));
        Assert.Equal(new[] { "Asc", "Desc" }, Enum.GetNames<SearchSortOrder>());
        Assert.Equal(new[] { 0, 1 }, Enum.GetValues<SearchSortOrder>().Select(value => (int)value));
        Assert.Equal(new[] { 0, 1, 2 }, Enum.GetValues<CreatePolygonsMode>().Select(value => (int)value));
    }

    private static readonly HashSet<string> IntentionalWireTypeDifferences =
    [
        // Processed observations still store dates as DateTime internally.
        $"{nameof(SOS.Api.Contracts.Observations.Responses.Observation)}.{nameof(SOS.Api.Contracts.Observations.Responses.Observation.Modified)}",
        $"{nameof(SOS.Api.Contracts.Observations.Responses.Observation)}.{nameof(SOS.Api.Contracts.Observations.Responses.Observation.Created)}",
        $"{nameof(SOS.Api.Contracts.Observations.Responses.Event)}.{nameof(SOS.Api.Contracts.Observations.Responses.Event.StartDate)}",
        $"{nameof(SOS.Api.Contracts.Observations.Responses.Event)}.{nameof(SOS.Api.Contracts.Observations.Responses.Event.EndDate)}",
        $"{nameof(SOS.Api.Contracts.Observations.Responses.Occurrence)}.{nameof(SOS.Api.Contracts.Observations.Responses.Occurrence.ReportedDate)}",

        // These enum types serialize as strings. Their wire representation is
        // verified separately from the EnumMember metadata.
        $"{nameof(DsDatasetDto)}.{nameof(DsDatasetDto.ProgrammeArea)}",
        $"{nameof(DsDatasetDto)}.{nameof(DsDatasetDto.Purpose)}",
        $"{nameof(DsDatasetDto)}.{nameof(DsDatasetDto.AccessRights)}",
        $"{nameof(DsProjectDto)}.{nameof(DsProjectDto.ProjectType)}"
    ];

    [Fact]
    public void ObservationContracts_MatchProcessedModelProperties()
    {
        using var assertionScope = new AssertionScope();

        AssertPropertiesMatch(
            typeof(SOS.Api.Contracts.Observations.Responses.ArtportalenInternal),
            typeof(SOS.Lib.Models.Processed.Observation.ArtportalenInternal));
        AssertPropertiesMatch(
            typeof(SOS.Api.Contracts.Observations.Responses.Observation),
            typeof(SOS.Lib.Models.Processed.Observation.Observation));
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
            typeof(SOS.Lib.Models.Processed.Observation.TaxonAttributes));
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
    public void AreaTypeEnum_MatchesDomainModel()
    {
        using var assertionScope = new AssertionScope();

        foreach (var areaType in Enum.GetValues<AreaTypeDto>())
        {
            Enum.TryParse<SOS.Lib.Enums.AreaType>(areaType.ToString(), out var domainAreaType)
                .Should().BeTrue();
            ((int)areaType).Should().Be((int)domainAreaType);
        }
    }

    [Fact]
    public void ExportJobInfo_ProcessingTimeIsTheDifferenceBetweenStartAndEnd()
    {
        var job = new ExportJobInfoDto
        {
            ProcessStartDate = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            ProcessEndDate = new DateTime(2026, 1, 1, 10, 5, 0, DateTimeKind.Utc)
        };

        job.ProcessingTime.Should().Be(TimeSpan.FromMinutes(5));

    }

    [Theory]
    [InlineData(typeof(SOS.Api.Contracts.Observations.Responses.Cloudiness), typeof(SOS.Lib.Enums.Weather.Cloudiness))]
    [InlineData(typeof(SOS.Api.Contracts.Observations.Responses.CompassDirection), typeof(SOS.Lib.Enums.CompassDirection))]
    [InlineData(typeof(SOS.Api.Contracts.Observations.Responses.Precipitation), typeof(SOS.Lib.Enums.Weather.Precipitation))]
    [InlineData(typeof(SOS.Api.Contracts.Observations.Responses.SnowCover), typeof(SOS.Lib.Enums.Weather.SnowCover))]
    [InlineData(typeof(SOS.Api.Contracts.Observations.Responses.Unit), typeof(SOS.Lib.Enums.Unit))]
    [InlineData(typeof(SOS.Api.Contracts.Observations.Responses.Visibility), typeof(SOS.Lib.Enums.Weather.Visibility))]
    [InlineData(typeof(SOS.Api.Contracts.Observations.Responses.WindStrength), typeof(SOS.Lib.Enums.Weather.WindStrength))]
    public void ObservationEnums_KeepServerMemberNamesAndLabels(Type contractType, Type serverType)
    {
        // The public enums already have different numeric values for some members.
        // Keep those values stable while restoring the shared descriptive labels.
        var contractFields = contractType.GetFields(BindingFlags.Public | BindingFlags.Static);
        var serverFields = serverType.GetFields(BindingFlags.Public | BindingFlags.Static);

        contractFields.Select(field => field.Name).Should().Equal(serverFields.Select(field => field.Name));
        foreach (var contractField in contractFields)
        {
            var serverField = serverType.GetField(contractField.Name)!;
            contractField.GetCustomAttribute<EnumMemberAttribute>()?.Value.Should().Be(
                serverField.GetCustomAttribute<EnumMemberAttribute>()?.Value);
        }
    }

    [Fact]
    public void DataStewardshipEnumMemberLabels_DoNotChangeExistingSystemTextJsonNames()
    {
        var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };

        JsonSerializer.Serialize(DsProgrammeArea.KustOchHav, options)
            .Should().Be("\"KustOchHav\"");
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

    [Fact]
    public void TaxonAggregationItem_PreservesExistingDateTimeJsonRepresentation()
    {
        var json = JsonSerializer.Serialize(new TaxonAggregationItemDto
        {
            FirstSighting = new DateTime(2026, 4, 5, 6, 7, 8, DateTimeKind.Unspecified),
            LastSighting = new DateTime(2026, 4, 6, 7, 8, 9, DateTimeKind.Unspecified)
        });

        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("FirstSighting").GetString()
            .Should().Be("2026-04-05T06:07:08");
        document.RootElement.GetProperty("LastSighting").GetString()
            .Should().Be("2026-04-06T07:08:09");
    }

    [Fact]
    public void ProcessInfo_UsesThePublicContractWithoutExposingHealthCheckFields()
    {
        var json = JsonSerializer.Serialize(new ProcessInfoDto
        {
            Start = new DateTimeOffset(2026, 4, 5, 6, 7, 8, TimeSpan.FromHours(2)),
            End = new DateTimeOffset(2026, 4, 5, 7, 8, 9, TimeSpan.FromHours(2)),
            ProvidersInfo =
            [
                new ProviderInfoDto
                {
                    DataProviderIdentifier = "provider",
                    ProcessFailCount = 3
                }
            ]
        });

        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("Start").GetString()
            .Should().Be("2026-04-05T06:07:08+02:00");
        document.RootElement.GetProperty("ProvidersInfo")[0]
            .TryGetProperty("ProcessFailCount", out _).Should().BeFalse();
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

}
