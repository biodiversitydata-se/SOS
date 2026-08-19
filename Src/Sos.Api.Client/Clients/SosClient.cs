using SOS.Api.Client.Abstractions;

namespace SOS.Api.Client.Clients;

/// <summary>
/// Aggregates the raw SOS API clients and higher-level workflows in one entry point.
/// </summary>
public sealed class SosClient
{
    /// <summary>Creates a combined entry point from the registered raw clients and workflow services.</summary>
    public SosClient(
        ISosObservationsClient observations,
        ISosObservationAggregationsClient observationAggregations,
        ISosAreasClient areas,
        ISosProjectsClient projects,
        ISosVocabulariesClient vocabularies,
        ISosDataProvidersClient dataProviders,
        ISosSystemsStatusInfoClient systemsStatusInfo,        
        ISosAnalysisClient analysis,
        ISosExportsClient exports,
        ISosDataStewardshipClient dataStewardship,
        ISosApiInfoClient apiInfo,
        ISosObservationSearchService observationSearch,
        ISosLookupService lookup,
        ISosStatusInfoService statusInfo,
        ISosAnalysisService analysisService,
        ISosExportService exportService,
        ISosWorkspaceFilterService workspaceFilters)
    {
        Observations = observations;
        ObservationAggregations = observationAggregations;
        Areas = areas;
        Projects = projects;
        Vocabularies = vocabularies;
        DataProviders = dataProviders;
        SystemsStatusInfo = systemsStatusInfo;
        Analysis = analysis;
        Exports = exports;
        DataStewardship = dataStewardship;
        ApiInfo = apiInfo;
        ObservationSearch = observationSearch;
        Lookup = lookup;
        StatusInfo = statusInfo;
        AnalysisService = analysisService;
        ExportService = exportService;
        WorkspaceFilters = workspaceFilters;
    }

    /// <summary>The raw observation client.</summary>
    public ISosObservationsClient Observations { get; }

    /// <summary>The raw observation-aggregation client.</summary>
    public ISosObservationAggregationsClient ObservationAggregations { get; }

    /// <summary>The raw area client.</summary>
    public ISosAreasClient Areas { get; }

    /// <summary>The raw project client.</summary>
    public ISosProjectsClient Projects { get; }

    /// <summary>The raw vocabulary client.</summary>
    public ISosVocabulariesClient Vocabularies { get; }

    /// <summary>The raw data-provider client.</summary>
    public ISosDataProvidersClient DataProviders { get; }

    /// <summary>The raw system-status client.</summary>
    public ISosSystemsStatusInfoClient SystemsStatusInfo { get; }    

    /// <summary>The raw analysis client.</summary>
    public ISosAnalysisClient Analysis { get; }

    /// <summary>The raw export client.</summary>
    public ISosExportsClient Exports { get; }

    /// <summary>The raw data-stewardship client.</summary>
    public ISosDataStewardshipClient DataStewardship { get; }

    /// <summary>The raw API-information client.</summary>
    public ISosApiInfoClient ApiInfo { get; }

    /// <summary>High-level observation-search workflows.</summary>
    public ISosObservationSearchService ObservationSearch { get; }

    /// <summary>High-level lookup workflows.</summary>
    public ISosLookupService Lookup { get; }

    /// <summary>High-level status-information workflows.</summary>
    public ISosStatusInfoService StatusInfo { get; }

    /// <summary>High-level analysis workflows.</summary>
    public ISosAnalysisService AnalysisService { get; }

    /// <summary>High-level export workflows.</summary>
    public ISosExportService ExportService { get; }

    /// <summary>The workspace-to-filter conversion service.</summary>
    public ISosWorkspaceFilterService WorkspaceFilters { get; }
}
