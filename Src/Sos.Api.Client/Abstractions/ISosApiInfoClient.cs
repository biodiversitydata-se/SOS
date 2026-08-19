using SOS.Api.Contracts.Metadata.Responses;

namespace SOS.Api.Client.Abstractions;

/// <summary>Client for SOS API information endpoints.</summary>
public interface ISosApiInfoClient
{
    /// <summary>Gets version and build information for the API.</summary>
    Task<ApiInformationDto?> GetApiInfoAsync(CancellationToken cancellationToken = default);
}
