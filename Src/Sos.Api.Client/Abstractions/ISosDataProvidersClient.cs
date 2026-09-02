using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Lookups.Responses;

namespace SOS.Api.Client.Abstractions;

/// <summary>Client for SOS data-provider lookup endpoints.</summary>
public interface ISosDataProvidersClient
{
    /// <summary>Gets data providers matching the supplied options.</summary>
    Task<IReadOnlyList<DataProviderDto>> GetDataProvidersAsync(
        DataProviderOptions? options = null,
        CancellationToken cancellationToken = default);
}
