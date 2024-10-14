using System.Threading.Tasks;

namespace SOS.Lib.Managers.Interfaces
{
    /// <summary>
    /// Cache manager for observation API
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// Clear requested cache
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        Task<bool> ClearAsync(Enums.Cache cache);
    }
}
