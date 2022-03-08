using SOS.Lib.Models.Processed.Observation;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Managers.Interfaces
{
    public interface IArtportalenApiManager
    {
        Task<Observation> GetObservationAsync(string occurrenceId);
        Task<Observation> GetObservationAsync(int sightingId);
    }
}
