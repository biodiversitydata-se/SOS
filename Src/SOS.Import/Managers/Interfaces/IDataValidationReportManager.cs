using System.Threading.Tasks;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Nors;

namespace SOS.Import.Managers.Interfaces
{
    public interface IDataValidationReportManager
    {
        public Task<DwcaDataValidationReport<object, Observation>> CreateDataValidationReport(
            DataProvider dataProvider,
            int maxNrObservationsToRead = 100000,
            int nrValidObservationsInReport = 10,
            int nrInvalidObservationsInReport = 100);
    }
}