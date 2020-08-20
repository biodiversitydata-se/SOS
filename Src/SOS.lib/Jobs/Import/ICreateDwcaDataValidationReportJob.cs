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
        ///     Run DwC-A compare
        /// </summary>
        /// <returns></returns>
        Task<string> RunAsync(string archivePath, IJobCancellationToken cancellationToken);
    }
}