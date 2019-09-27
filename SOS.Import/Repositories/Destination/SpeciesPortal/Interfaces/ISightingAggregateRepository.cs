using SOS.Import.Models.Aggregates;
using SOS.Import.Repositories.Destination.Interfaces;

namespace SOS.Import.Repositories.Destination.SpeciesPortal.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISightingAggregateRepository : IAggregateRepository<APSightingVerbatim, int>
    {
    }
}
