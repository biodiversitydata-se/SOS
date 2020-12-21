using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Jobs.Import;

namespace SOS.Import.Jobs
{
    /// <summary>
    ///     Create data validation report job for a specific data provider.
    /// </summary>
    public class DataValidationReportJob : IDataValidationReportJob
    {
        public async Task<string> RunAsync(
            string dataProviderIdentifier, 
            int maxNrObservationsToRead, 
            int nrValidObservationsInReport,
            int nrInvalidObservationsInReport, 
            IJobCancellationToken cancellationToken)
        {
            return "Not Implemented yet";
        }
    }
}