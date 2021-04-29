using MongoDB.Bson;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;

namespace SOS.Lib.Repositories.Verbatim.Interfaces
{   
    /// <summary>
    /// 
    /// </summary>
    public interface
        IVirtualHerbariumObservationVerbatimRepository : IVerbatimRepositoryBase<VirtualHerbariumObservationVerbatim,
            int>
    {
    }
}