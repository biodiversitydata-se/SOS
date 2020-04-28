using MongoDB.Bson;
using SOS.Lib.Models.Verbatim.Shark;

namespace SOS.Process.Repositories.Source.Interfaces
{
    public interface ISharkObservationVerbatimRepository : IVerbatimBaseRepository<SharkObservationVerbatim, ObjectId>
    {
    }
}
