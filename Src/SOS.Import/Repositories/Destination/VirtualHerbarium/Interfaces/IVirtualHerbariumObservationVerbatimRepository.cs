using MongoDB.Bson;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;

namespace SOS.Import.Repositories.Destination.VirtualHerbarium.Interfaces
{
    public interface IVirtualHerbariumObservationVerbatimRepository : IVerbatimRepository<VirtualHerbariumObservationVerbatim, ObjectId>
    {
    }
}