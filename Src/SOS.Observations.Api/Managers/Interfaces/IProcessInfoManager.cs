using SOS.Observations.Api.Dtos;
using System.Threading.Tasks;

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