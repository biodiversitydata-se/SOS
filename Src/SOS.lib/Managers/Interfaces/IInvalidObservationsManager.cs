using System.Threading.Tasks;

namespace SOS.Lib.Managers.Interfaces
{
    public interface IInvalidObservationsManager
    {
        Task<bool> CreateExcelFileReportAsync(string reportId, int dataProviderId, string createdBy);
    }
}
