using System.Collections.Generic;
using System.Threading.Tasks;
using NetTopologySuite.Features;
using SOS.Lib.Models.Gis;

namespace SOS.Import.Services.Interfaces
{
    public interface IGeoRegionApiService
    {
        Task<IEnumerable<AreaDataset>> GetAreaDatasets();
        Task<FeatureCollection> GetFeatureCollectionFromZipAsync(IEnumerable<int> areaDatasetIds, int srid = 4326);

        Task<FeatureCollection> GetFeatureCollectionWithAllAreasAsync(
            int srid = 4326);
    }
}