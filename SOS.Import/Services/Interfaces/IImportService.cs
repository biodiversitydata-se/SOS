using System.Threading.Tasks;

namespace SOS.Import.Services.Interfaces
{
    /// <summary>
    /// Main Service interface
    /// </summary>
    public interface IImportService
    {
        /// <summary>
        /// Start import of sightings
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        Task<bool> ImportAsync(int sources);
    }
}
