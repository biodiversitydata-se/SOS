using System.ComponentModel;
using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Import
{
    public interface IApiUsageStatisticsHarvestJob
    {
        /// <summary>
        ///     Run harvest API usage statistics.
        /// </summary>
        /// <returns></returns>
        [DisplayName("Harvest API usage statistics")]
        Task<bool> RunHarvestStatisticsAsync();

        /// <summary>
        ///     Run create usage statistics Excel file.
        /// </summary>
        /// <param name="reportId"></param>
        /// <param name="createdBy"></param>
        /// <returns></returns>
        [DisplayName("Create API usage statistics Excel file, Id: \"{0}\"")]
        Task<bool> RunCreateExcelFileReportAsync(string reportId, string createdBy);
    }
}