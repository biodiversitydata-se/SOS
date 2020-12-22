using System.Threading.Tasks;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.Validation.Interfaces
{
    public interface IDataValidationReportFactory
    {
        public Task<DwcaDataValidationReport<object, Observation>> CreateDataValidationSummary(
            DataProvider dataProvider,
            int maxNrObservationsToRead = 100000,
            int nrValidObservationsInReport = 10,
            int nrInvalidObservationsInReport = 100);
    }
}