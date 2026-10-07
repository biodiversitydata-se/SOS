using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Areas.Requests;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Common.Responses;

namespace SOS.Api.Client.Abstractions;

/// <summary>High-level area lookup workflows.</summary>
public interface ISosLookupService
{
    /// <summary>Gets one area together with its exported geometry.</summary>
    Task<AreaWithGeometryDto?> GetAreaWithGeometryAsync(
        AreaTypeDto areaType,
        string featureId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets areas together with exported geometries.</summary>
    Task<PagedResultDto<AreaWithGeometryDto>> GetAreasWithGeometryAsync(
        AreasOptions? options = null,
        CancellationToken cancellationToken = default);
}
