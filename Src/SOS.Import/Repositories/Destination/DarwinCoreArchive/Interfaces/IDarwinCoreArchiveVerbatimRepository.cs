using MongoDB.Bson;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.Repositories.Destination.DarwinCoreArchive.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDarwinCoreArchiveVerbatimRepository : IVerbatimRepository<DarwinCoreObservationVerbatim, ObjectId>
    {

    }
}
