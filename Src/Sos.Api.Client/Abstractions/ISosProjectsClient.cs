using SOS.Api.Contracts.Lookups.Responses;

namespace SOS.Api.Client.Abstractions;

/// <summary>Client for SOS project lookup endpoints.</summary>
public interface ISosProjectsClient
{
    /// <summary>Gets projects matching an optional text filter.</summary>
    Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(
        string? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets projects with the specified identifiers.</summary>
    Task<IReadOnlyList<ProjectDto>> GetProjectsByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one project by identifier.</summary>
    Task<ProjectDto?> GetProjectAsync(
        int id,
        CancellationToken cancellationToken = default);
}
