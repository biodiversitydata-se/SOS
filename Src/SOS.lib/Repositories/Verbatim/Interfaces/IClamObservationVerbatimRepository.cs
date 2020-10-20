using MongoDB.Bson;
using SOS.Lib.Models.Verbatim.ClamPortal;

namespace SOS.Lib.Repositories.Verbatim.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IClamObservationVerbatimRepository : IVerbatimRepositoryBase<ClamObservationVerbatim, ObjectId>
    {
    }
}