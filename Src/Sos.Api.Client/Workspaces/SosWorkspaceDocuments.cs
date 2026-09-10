using System.Text.Json;
using SOS.Api.Client.Exceptions;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Workspaces;

namespace SOS.Api.Client.Workspaces;

/// <summary>Reads, writes and validates portable workspace documents without network access.</summary>
public static class SosWorkspaceDocuments
{
    /// <summary>Reads a supported document. Missing versions, invalid JSON and invalid selections produce a workspace validation exception.</summary>
    public static SosWorkspaceDocument Deserialize(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            using var parsed = JsonDocument.Parse(json);
            if (parsed.RootElement.ValueKind != JsonValueKind.Object ||
                !parsed.RootElement.TryGetProperty("schemaVersion", out var version) ||
                version.ValueKind != JsonValueKind.Number || !version.TryGetInt32(out var number))
                throw Error("SchemaVersion", "A workspace must contain an integer schemaVersion.");
            ValidateVersion(number);
            var document = JsonSerializer.Deserialize<SosWorkspaceDocument>(json)!;
            Validate(document);
            return document;
        }
        catch (JsonException exception)
        {
            throw new SosWorkspaceFilterValidationException("Document",
                "The workspace JSON is invalid: " + exception.Message, innerException: exception);
        }
    }

    /// <summary>Validates and serializes a document, preserving unknown document metadata.</summary>
    public static string Serialize(SosWorkspaceDocument document)
    {
        Validate(document);
        return JsonSerializer.Serialize(document);
    }

    /// <summary>Checks the schema and local search rules before saving. Does not check external taxon existence or access rights.</summary>
    public static void Validate(SosWorkspaceDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        ValidateVersion(document.SchemaVersion);
        if (document.Search is null) throw Error("Search", "A workspace must contain a search definition.");
        ValidateSearch(document.Search);
    }

    /// <summary>Validates locally checkable search selections without resolving taxa.</summary>
    public static void ValidateSearch(SosWorkspaceSearchDefinition search)
    {
        ArgumentNullException.ThrowIfNull(search);
        var date = search.DateFilter;
        if (date is not null)
        {
            CheckEnum(date.Mode, nameof(date.Mode));
            if (date.Mode == WorkspaceDateFilterMode.Custom)
            {
                if (!date.IgnoreYear && date.StartDate?.Date > date.EndDate?.Date)
                    throw Error("StartDate", "StartDate must be less than or equal to EndDate.");
                if (date.IgnoreYear)
                {
                    if (date.StartYear is < 1 or > 9999 || date.EndYear is < 1 or > 9999)
                        throw Error("StartYear", "Year boundaries must be between 1 and 9999.");
                    if (date.StartYear > date.EndYear)
                        throw Error("StartYear", "StartYear must be less than or equal to EndYear.");
                }
            }
        }
        if (search.ObservationDetails is { } details)
        {
            CheckEnum(details.Presence, nameof(details.Presence));
            CheckEnum(details.Spontaneous, nameof(details.Spontaneous));
            CheckEnum(details.ExtendedTypeFilter, nameof(details.ExtendedTypeFilter));
        }
        foreach (var area in search.Areas ?? [])
            if (area is null || !Enum.TryParse<AreaTypeDto>(area.AreaType, true, out var type) || !Enum.IsDefined(type))
                throw Error("AreaType", "The workspace contains an unsupported area type.");
    }

    private static void ValidateVersion(int version)
    {
        if (version != SosWorkspaceDocument.CurrentSchemaVersion)
            throw new SosWorkspaceFilterValidationException("SchemaVersion",
                $"Workspace schema version '{version}' is not supported.",
                [SosWorkspaceDocument.CurrentSchemaVersion.ToString(System.Globalization.CultureInfo.InvariantCulture)]);
    }

    private static void CheckEnum<T>(T value, string name) where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new SosWorkspaceFilterValidationException(name, $"Value '{value}' is not supported for {name}.", Enum.GetNames<T>());
    }

    private static SosWorkspaceFilterValidationException Error(string name, string message) => new(name, message);
}
