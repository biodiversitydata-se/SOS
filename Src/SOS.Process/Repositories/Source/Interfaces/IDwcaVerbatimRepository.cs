using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Process.Repositories.Source.Interfaces
{
    public interface IDwcaVerbatimRepository : IVerbatimBaseRepository<DwcObservationVerbatim, ObjectId>
    {
        Task<IAsyncCursor<DwcObservationVerbatim>> GetAllByCursorAsync(
            int dataProviderId,
            string dataProviderIdentifier);

        Task<List<DwcObservationVerbatim>> GetAllAsync(
            int dataProviderId,
            string dataProviderIdentifier);
    }
}