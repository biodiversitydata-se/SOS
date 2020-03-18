using System.Threading.Tasks;
using SOS.Lib.Enums;

namespace SOS.Process.Managers.Interfaces
{
    /// <summary>
    /// Processed data factory
    /// </summary>
    public interface IInstanceManager
    {
        /// <summary>
        /// Copy data from active to inactive instance
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        Task<bool> CopyProviderDataAsync(DataProvider provider);

        /// <summary>
        /// Activate passed instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        Task<bool> SetActiveInstanceAsync(byte instance);
    }
}
