using System.Threading.Tasks;

namespace SOS.Observations.Api.Managers.Interfaces
{
    public interface IDevOpsManager
    {
        /// <summary>
        /// Get build info
        /// </summary>
        /// <returns></returns>
        Task<string> GetBuildInfoAsync();
    }
}
