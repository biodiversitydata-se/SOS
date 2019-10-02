using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Core.Models.DOI;
using SOS.Core.Models.Observations;

namespace SOS.Core.Services
{
    public interface IDoiService
    {
        Task<DoiInfo> CreateDoiAsync(
            IEnumerable<ObservationVersionIdentifier> observationIdentifiers,
            DoiMetadata doiMetadata);
        Task<IEnumerable<ObservationVersionIdentifier>> GetDoiObservationIdentifiersAsync(string filename);
        Task<IList<ProcessedDwcObservation>> GetDoiObservationsAsync(string filename);
    }
}