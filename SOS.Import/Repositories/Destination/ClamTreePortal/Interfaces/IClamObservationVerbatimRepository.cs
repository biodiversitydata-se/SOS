using MongoDB.Bson;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.Verbatim.ClamTreePortal;

namespace SOS.Import.Repositories.Destination.ClamTreePortal.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IClamObservationVerbatimRepository : IVerbatimRepository<ClamObservationVerbatim, ObjectId>
    {

    }
}
