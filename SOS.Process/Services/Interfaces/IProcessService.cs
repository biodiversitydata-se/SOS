using System.Threading.Tasks;

namespace SOS.Process.Services.Interfaces
{
    /// <summary>
    /// Main Service interface
    /// </summary>
    public interface IProcessService
    {
        /// <summary>
        /// Start import of sightings
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        Task<bool> ImportAsync(int sources);
    }
}
