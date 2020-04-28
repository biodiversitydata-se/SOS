using MongoDB.Bson;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.Verbatim.Shark;

namespace SOS.Import.Repositories.Destination.Shark.Interfaces
{
    public interface ISharkObservationVerbatimRepository : IVerbatimRepository<SharkObservationVerbatim, ObjectId>
    {
    }
}