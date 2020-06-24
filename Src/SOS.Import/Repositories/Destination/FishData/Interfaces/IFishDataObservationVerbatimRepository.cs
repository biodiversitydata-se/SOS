using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.Verbatim.FishData;

namespace SOS.Import.Repositories.Destination.FishData.Interfaces
{
    public interface IFishDataObservationVerbatimRepository : IVerbatimRepository<FishDataObservationVerbatim, string>
    {
    }
}