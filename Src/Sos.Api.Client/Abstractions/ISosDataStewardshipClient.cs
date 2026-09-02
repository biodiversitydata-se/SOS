using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.DataStewardship.Responses;
using SOS.Api.Contracts.Observations.Requests;

namespace SOS.Api.Client.Abstractions;

/// <summary>Client for SOS data-stewardship dataset endpoints.</summary>
public interface ISosDataStewardshipClient
{
    /// <summary>Gets a dataset by identifier.</summary>
    Task<DsDatasetDto?> GetDatasetAsync(
        string id,
        SosRoleRequestOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets datasets matching a textual dataset filter.</summary>
    Task<PagedResultDto<DsDatasetDto>?> GetDatasetsByFilterAsync(
        string filter,
        DataStewardshipDatasetOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets datasets associated with observations matching a search filter.</summary>
    Task<PagedResultDto<DsDatasetDto>?> SearchDatasetsAsync(
        SearchFilterBaseDto filter,
        DataStewardshipDatasetOptions? options = null,
        CancellationToken cancellationToken = default);
}
