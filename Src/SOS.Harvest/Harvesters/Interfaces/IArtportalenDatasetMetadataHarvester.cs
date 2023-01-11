using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Harvest.Harvesters.Interfaces
{
    public interface IArtportalenDatasetMetadataHarvester
    {
        public Task<HarvestInfo> HarvestDatasetsAsync();
    }
}