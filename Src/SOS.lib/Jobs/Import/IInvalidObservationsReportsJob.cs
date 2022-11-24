using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Import
{
    public interface IInvalidObservationsReportsJob
    {
        /// <summary>
        ///     Run create invalid observations Excel file.
        /// </summary>
        /// <param name="reportId"></param>
        /// <param name="dataProviderId"></param>
        /// <param name="createdBy"></param>
        /// <returns></returns>
        [JobDisplayName("Create Invalid Observations Excel file, Id: \"{0}\"")]
        [Queue("low")]
        Task<bool> RunCreateExcelFileReportAsync(string reportId, int dataProviderId, string createdBy);
    }
}