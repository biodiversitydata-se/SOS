using System.Threading.Tasks;
using SOS.Lib.Models.Shared;

namespace SOS.Process.Managers.Interfaces
{
    /// <summary>
    ///     Instance manager interface
    /// </summary>
    public interface IInstanceManager
    {
        /// <summary>
        ///     Copy data from active to inactive instance.
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <returns></returns>
        Task<bool> CopyProviderDataAsync(DataProvider dataProvider);

        /// <summary>
        ///     Activate passed instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        Task<bool> SetActiveInstanceAsync(byte instance);
    }
}