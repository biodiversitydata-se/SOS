using MongoDB.Bson;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Lib.Models.Verbatim.Kul;

namespace SOS.Process.Repositories.Source.Interfaces
{
    public interface IKulObservationVerbatimRepository : IVerbatimBaseRepository<KulObservationVerbatim, string>
    {
    }
}
