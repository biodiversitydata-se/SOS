using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.Verbatim.Nors;

namespace SOS.Import.Repositories.Destination.Nors.Interfaces
{
    public interface INorsObservationVerbatimRepository : IVerbatimRepository<NorsObservationVerbatim, string>
    {
    }
}