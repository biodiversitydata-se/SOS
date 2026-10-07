using SOS.Api.Contracts.Metadata.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Taxonomy.Requests;
using SOS.Api.Contracts.Status.Responses;

namespace SOS.Api.Client.Abstractions;

/// <summary>Client for SOS system-status endpoints.</summary>
public interface ISosSystemsStatusInfoClient
{
    /// <summary>Gets a summary of observation processing.</summary>
    Task<ProcessSummaryDto?> GetProcessSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets information about the active or inactive observation process.</summary>
    Task<ProcessInfoDto?> GetProcessInfoAsync(bool active, CancellationToken cancellationToken = default);

    /// <summary>Gets processing information from MongoDB.</summary>
    Task<IEnumerable<MongoDbProcessInfoDto>?> GetMongoDbProcessInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the currently active processing instance.</summary>
    Task<ActiveInstanceInfoDto?> GetActiveInstanceAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets search-index allocation information.</summary>
    Task<SearchIndexInfoDto?> GetSearchIndexInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets MongoDB disk-usage information.</summary>
    Task<MongoDbInfoDto?> GetMongoDatabaseInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the system health report.</summary>
    Task<HealthReportDto?> GetHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a diagram describing relationships for the supplied taxa.</summary>
    Task<string?> GetTaxonRelationsDiagramAsync(
        int[] taxonIds,
        TaxonRelationsTreeIterationMode treeIterationMode = TaxonRelationsTreeIterationMode.BothParentsAndChildren,
        bool includeSecondaryRelations = false,
        DiagramFormat diagramFormat = DiagramFormat.Mermaid,
        TaxonomyType taxonomyType = TaxonomyType.Dyntaxa,
        string translationCultureCode = "sv-SE",
        CancellationToken cancellationToken = default);
}
