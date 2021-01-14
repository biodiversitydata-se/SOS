using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Interfaces;

namespace SOS.Lib.Repositories.Resource.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IReportRepository : IRepositoryBase<Report, string>
    {
        ///// <summary>
        /////     Create indexes
        ///// </summary>
        ///// <returns></returns>
        //Task CreateIndexAsync();

        ///// <summary>
        /////     Delete all geometries stored in Gridfs
        ///// </summary>
        ///// <returns></returns>
        //Task DropGeometriesAsync();

        ///// <summary>
        /////  Get the geometry for a area
        ///// </summary>
        ///// <param name="areaType"></param>
        ///// <param name="featureId"></param>
        ///// <returns></returns>
        //Task<IGeoShape> GetGeometryAsync(AreaType areaType, string featureId);

        ///// <summary>
        /////     Get all the areas, paged
        ///// </summary>
        ///// <param name="areaTypes">Skip this many</param>
        ///// <param name="searchString">Skip this many</param>
        ///// <param name="skip">Skip this many</param>
        ///// <param name="take">Take this many areas</param>
        ///// <returns></returns>
        //Task<PagedResult<Area>> GetAreasAsync(IEnumerable<AreaType> areaTypes, string searchString, int skip,
        //    int take);

        //Task<List<Area>> GetAsync(AreaType[] areaTypes);

        ///// <summary>
        ///// Get Area by type and feature
        ///// </summary>
        ///// <param name="areaType"></param>
        ///// <param name="featureId"></param>
        ///// <returns></returns>
        //Task<Area> GetAsync(AreaType areaType, string featureId);
        //Task<ReportFile> GetAsync(string id);

        Task<bool> StoreFileAsync(string filename, byte[] file);
        Task<byte[]> GetFileAsync(string filename);
        Task<bool> DeleteFileAsync(string filename);
        Task<bool> DeleteFilesAsync(IEnumerable<string> filenames);
    }
}