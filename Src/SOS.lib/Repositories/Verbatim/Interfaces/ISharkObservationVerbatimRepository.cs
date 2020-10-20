using MongoDB.Bson;
using SOS.Lib.Models.Verbatim.Shark;

namespace SOS.Lib.Repositories.Verbatim.Interfaces
{
    public interface ISharkObservationVerbatimRepository : IVerbatimRepositoryBase<SharkObservationVerbatim, ObjectId>
    {
    }
}