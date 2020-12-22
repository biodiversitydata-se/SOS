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
        ///     Run create DwC-A validation report job.
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