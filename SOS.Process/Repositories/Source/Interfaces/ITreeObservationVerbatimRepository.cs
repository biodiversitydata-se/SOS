using MongoDB.Bson;
using SOS.Lib.Models.Verbatim.ClamTreePortal;

namespace SOS.Process.Repositories.Source.Interfaces
{
    public interface ITreeObservationVerbatimRepository : IVerbatimBaseRepository<TreeObservationVerbatim, ObjectId>
    {
    }
}
