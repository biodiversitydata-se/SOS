using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Interfaces;

namespace SOS.Lib.Repositories.Resource.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IReportRepository : IRepositoryBase<Report, string>
    {
        Task<bool> StoreFileAsync(string filename, byte[] file);
        Task<byte[]> GetFileAsync(string filename);
        Task<bool> DeleteFileAsync(string filename);
        Task<bool> DeleteFilesAsync(IEnumerable<string> filenames);
    }
}