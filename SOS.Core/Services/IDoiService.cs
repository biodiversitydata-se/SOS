using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Core.Models.DOI;
using SOS.Core.Models.Observations;

namespace SOS.Core.Services
{
    public interface IDoiService
    {
        Task<DoiInfo> CreateDoiWithAllObservationsAsync(DoiMetadata doiMetadata);
        Task<IEnumerable<ObservationVersionIdentifier>> GetDoiObservationsAsync(string filename);
    }
}