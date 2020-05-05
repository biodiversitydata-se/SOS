using MongoDB.Bson;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;

namespace SOS.Process.Repositories.Source.Interfaces
{
    public interface IVirtualHerbariumObservationVerbatimRepository : IVerbatimBaseRepository<VirtualHerbariumObservationVerbatim, ObjectId>
    {
    }
}
