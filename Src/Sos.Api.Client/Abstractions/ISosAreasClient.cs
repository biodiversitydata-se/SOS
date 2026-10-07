using SOS.Api.Client.Requests;
using SOS.Api.Contracts.Areas.Requests;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Common.Responses;

namespace SOS.Api.Client.Abstractions;

/// <summary>Client for SOS area lookup and export endpoints.</summary>
public interface ISosAreasClient
{
    /// <summary>Gets a filtered page of areas.</summary>
    Task<PagedResultDto<AreaBaseDto>?> GetAreasAsync(
        AreasOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one area by type and feature identifier.</summary>
    Task<AreaBaseDto?> GetAreaAsync(
        AreaTypeDto areaType,
        string featureId,
        CancellationToken cancellationToken = default);

    /// <summary>Exports one area's geometry in the requested format.</summary>
    Task<byte[]?> GetAreaExportAsync(
        AreaTypeDto areaType,
        string featureId,
        AreaExportFormatDto format = AreaExportFormatDto.Json,
        CancellationToken cancellationToken = default);
}
