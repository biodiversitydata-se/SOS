using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Import
{
    /// <summary>
    /// Interface for CreateDwcaDataValidationReportJob.
    /// </summary>
    public interface ICreateDwcaDataValidationReportJob
    {
        /// <summary>
        ///     Run create DwC-A validation report job.
        /// </summary>
        /// <returns></returns>
        Task<string> RunAsync(
            string archivePath,
            int maxNrObservationsToRead,
            int nrValidObservationsInReport,
            int nrInvalidObservationsInReport,
            IJobCancellationToken cancellationToken);
    }
}