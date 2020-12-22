using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Import
{
    /// <summary>
    /// Interface for DataValidationReportJob.
    /// </summary>
    public interface IDataValidationReportJob
    {
        /// <summary>
        ///     Run create a data validation report job for a specific data provider.
        /// </summary>
        /// <returns></returns>
        [DisplayName("Data Validation Report, Data provider: \"{0}\"")]
        Task<string> RunAsync(
            string dataProviderIdentifier,
            int maxNrObservationsToRead,
            int nrValidObservationsInReport,
            int nrInvalidObservationsInReport,
            IJobCancellationToken cancellationToken);
    }
}