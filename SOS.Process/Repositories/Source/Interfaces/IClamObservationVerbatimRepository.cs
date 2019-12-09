using MongoDB.Bson;
using SOS.Lib.Models.Verbatim.ClamPortal;

namespace SOS.Process.Repositories.Source.Interfaces
{
    public interface IClamObservationVerbatimRepository : IVerbatimBaseRepository<ClamObservationVerbatim, ObjectId>
    {
    }
}
