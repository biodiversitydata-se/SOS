using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.DarwinCore;

namespace SOS.Import.Repositories.Destination.Taxon.Interfaces
{
    /// <summary>
    /// </summary>
    public interface ITaxonVerbatimRepository : IVerbatimRepository<DarwinCoreTaxon, int>
    {
    }
}