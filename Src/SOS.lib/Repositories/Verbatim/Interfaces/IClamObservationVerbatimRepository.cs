using MongoDB.Bson;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Lib.Repositories.Interfaces;

namespace SOS.Lib.Repositories.Verbatim.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IClamObservationVerbatimRepository : IRepositoryBase<ClamObservationVerbatim, ObjectId>
    {
    }
}