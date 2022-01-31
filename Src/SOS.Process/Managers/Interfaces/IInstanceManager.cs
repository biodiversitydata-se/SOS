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
        ///     Activate passed instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        Task<bool> SetActiveInstanceAsync(byte instance);
    }
}