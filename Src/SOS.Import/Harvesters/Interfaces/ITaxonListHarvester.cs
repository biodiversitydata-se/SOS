using System.Threading.Tasks;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Interfaces
{
    /// <summary>
    ///     Interface for harvest taxon lists.
    /// </summary>
    public interface ITaxonListHarvester
    {
        public Task<HarvestInfo> HarvestTaxonListsAsync();
    }
}