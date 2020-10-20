using MongoDB.Bson;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;

namespace SOS.Lib.Repositories.Verbatim.Interfaces
{
    public interface
        IVirtualHerbariumObservationVerbatimRepository : IVerbatimRepositoryBase<VirtualHerbariumObservationVerbatim,
            ObjectId>
    {
    }
}