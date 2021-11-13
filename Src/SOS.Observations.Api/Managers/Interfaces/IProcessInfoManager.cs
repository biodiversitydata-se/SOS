using System.Threading.Tasks;
using SOS.Observations.Api.Dtos;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    ///     Process information manager
    /// </summary>
    public interface IProcessInfoManager
    {
        /// <summary>
        ///     Get process information
        /// </summary>
        /// <returns></returns>
        Task<ProcessInfoDto> GetProcessInfoAsync(string id);
    }
}