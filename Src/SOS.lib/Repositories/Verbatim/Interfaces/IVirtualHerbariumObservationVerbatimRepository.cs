using MongoDB.Bson;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;
using SOS.Lib.Repositories.Interfaces;

namespace SOS.Lib.Repositories.Verbatim.Interfaces
{
    public interface
        IVirtualHerbariumObservationVerbatimRepository : IRepositoryBase<VirtualHerbariumObservationVerbatim,
            ObjectId>
    {
    }
}