﻿using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Shared;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    ///     Area manager
    /// </summary>
    public interface IAreaManager
    {
        /// <summary>
        /// Get areas by id
        /// </summary>
        /// <param name="areaKeys"></param>
        /// <returns></returns>
        Task<IEnumerable<AreaBaseDto>> GetAreasAsync(IEnumerable<(AreaTypeDto, string)> areaKeys);

        /// <summary>
        ///     Get areas matching provided filter
        /// </summary>
        /// <param name="areaTypes"></param>
        /// <param name="searchString"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedResult<AreaBaseDto>> GetAreasAsync(IEnumerable<AreaTypeDto> areaTypes, string searchString,
            int skip, int take);

        /// <summary>
        /// Get a single area
        /// </summary>
        /// <param name="areaType"></param>
        /// <param name="featureId"></param>
        /// <returns></returns>
        Task<AreaBaseDto> GetAreaAsync(AreaTypeDto areaType, string featureId);

        /// <summary>
        /// Get area as zip file in specified format.
        /// </summary>
        /// <param name="areaType"></param>
        /// <param name="featureId"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        Task<byte[]> GetZippedAreaAsync(AreaTypeDto areaType, string featureId, AreaExportFormat format);

        /// <summary>
        /// Get a area geometry
        /// </summary>
        /// <param name="areaType"></param>
        /// <param name="featureId"></param>
        /// <returns></returns>
        Task<IGeoShape> GetGeometryAsync(AreaType areaType, string featureId);

        /// <summary>
        /// Get multiple geometries
        /// </summary>
        /// <param name="areaKeys"></param>
        /// <returns></returns>
        Task<IEnumerable<IGeoShape>> GetGeometriesAsync(IEnumerable<(AreaType areaType, string featureId)> areaKeys);
    }
}