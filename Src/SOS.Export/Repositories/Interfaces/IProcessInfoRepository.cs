using System.Threading.Tasks;
using SOS.Lib.Models.Processed.ProcessInfo;

namespace SOS.Export.Repositories.Interfaces
{
    /// <summary>
    /// Processed data class
    /// </summary>
    public interface IProcessInfoRepository : IBaseRepository<ProcessInfo, byte>
    {

    }
}
