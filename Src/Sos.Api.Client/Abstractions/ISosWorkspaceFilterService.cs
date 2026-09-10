using SOS.Api.Client.Workspaces;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Workspaces;

namespace SOS.Api.Client.Abstractions;

/// <summary>Converts application-independent workspace settings into SOS search filters.</summary>
public interface ISosWorkspaceFilterService
{
    /// <summary>Creates an observation search filter from a supported workspace document.</summary>
    Task<SearchFilterInternalDto> CreateSearchFilterAsync(
        SosWorkspaceDocument workspace,
        WorkspaceFilterOverrides? overrides = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates an aggregation or analysis filter from a supported workspace document.</summary>
    Task<SearchFilterAggregationInternalDto> CreateAggregationFilterAsync(
        SosWorkspaceDocument workspace,
        WorkspaceFilterOverrides? overrides = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates an observation search filter from a workspace definition.</summary>
    Task<SearchFilterInternalDto> CreateSearchFilterAsync(
        SosWorkspaceSearchDefinition workspace,
        WorkspaceFilterOverrides? overrides = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates an aggregation filter from a workspace definition.</summary>
    Task<SearchFilterAggregationInternalDto> CreateAggregationFilterAsync(
        SosWorkspaceSearchDefinition workspace,
        WorkspaceFilterOverrides? overrides = null,
        CancellationToken cancellationToken = default);
}
