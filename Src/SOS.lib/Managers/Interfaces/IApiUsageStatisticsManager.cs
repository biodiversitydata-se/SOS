using System.Threading.Tasks;

namespace SOS.Lib.Managers.Interfaces
{
    public interface IApiUsageStatisticsManager
    {
        Task<bool> HarvestStatisticsAsync();
        Task<bool> CreateExcelFileReportAsync(string reportId, string createdBy);
    }
}
