using SOS.Process.Models.Aggregates;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Repositories.Destination.SpeciesPortal.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISightingAggregateRepository : IAggregateRepository<APSightingVerbatim, int>
    {
    }
}
