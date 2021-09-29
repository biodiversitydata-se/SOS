using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Models.Shared;

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
        [DisplayName("Data Validation Report, Id: \"{0}\",  Data provider: \"{2}\"")]
        [Queue("low")]
        Task<Report> RunAsync(
            string reportId, 
            string createdBy, 
            string dataProviderIdentifier,
            int maxNrObservationsToRead,
            int nrValidObservationsInReport,
            int nrInvalidObservationsInReport,
            IJobCancellationToken cancellationToken);
    }
}