using MongoDB.Bson;
using SOS.Lib.Models.Verbatim.Shark;
using SOS.Lib.Repositories.Interfaces;

namespace SOS.Lib.Repositories.Verbatim.Interfaces
{
    public interface ISharkObservationVerbatimRepository : IRepositoryBase<SharkObservationVerbatim, ObjectId>
    {
    }
}