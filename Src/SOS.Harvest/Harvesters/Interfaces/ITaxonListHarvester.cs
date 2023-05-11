using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Harvest.Harvesters.Interfaces
{
    /// <summary>
    ///     Interface for harvest taxon lists.
    /// </summary>
    public interface ITaxonListHarvester
    {
        public Task<HarvestInfo> HarvestTaxonListsAsync();
    }
}