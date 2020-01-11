using MongoDB.Bson;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.Verbatim.ClamPortal;

namespace SOS.Import.Repositories.Destination.ClamPortal.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IClamObservationVerbatimRepository : IVerbatimRepository<ClamObservationVerbatim, ObjectId>
    {

    }
}
