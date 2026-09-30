using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Lookups.Responses;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;

namespace SOS.Api.Client.Abstractions;

/// <summary>Client for SOS data-provider lookup endpoints.</summary>
public interface ISosDataProvidersClient
{
    /// <summary>Gets data providers matching the supplied options.</summary>
    Task<IReadOnlyList<DataProviderDto>> GetDataProvidersAsync(
        DataProviderOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the EML (Electronic Mailing List) for a specific data provider.
    /// </summary>
    /// <param name="dataProviderId">The ID of the data provider.</param>
    /// <param name="options">Options for retrieving the EML.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The EML as an XML document, or null if not found.</returns>
    Task<XmlDocument?> GetDataProviderEmlAsXmlAsync(
        int dataProviderId,
        DataProviderEmlOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the EML (Electronic Mailing List) for a specific data provider.
    /// </summary>
    /// <param name="dataProviderId">The ID of the data provider.</param>
    /// <param name="options">Options for retrieving the EML.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The EML as a JSON document, or null if not found.</returns>
    Task<JsonObject?> GetDataProviderEmlAsJsonAsync(
        int dataProviderId,
        DataProviderEmlOptions? options = null,
        CancellationToken cancellationToken = default);
}
