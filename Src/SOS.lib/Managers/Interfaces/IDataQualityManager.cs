using System.Threading.Tasks;
using SOS.Lib.Models.DataQuality;

namespace SOS.Lib.Managers.Interfaces
{
    /// <summary>
    /// Interface for Data Quality Manager
    /// </summary>
    public interface IDataQualityManager
    {
        /// <summary>
        /// Get data quality report
        /// </summary>
        /// <returns></returns>
        Task<DataQualityReport> GetReportAsync();
    }
}
