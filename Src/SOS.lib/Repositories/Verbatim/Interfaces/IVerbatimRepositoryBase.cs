using System.IO;
using System.Threading.Tasks;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Repositories.Interfaces;

namespace SOS.Lib.Repositories.Verbatim.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IVerbatimRepositoryBase<TEntity, TKey> : IRepositoryBase<TEntity, TKey> where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// Make collection permanent
        /// </summary>
        /// <returns></returns>
        Task<bool> PermanentizeCollectionAsync();

        /// <summary>
        /// Get source file for provider
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        Task<Stream> GetSourceFileAsync(int providerId);

        /// <summary>
        /// Store verbatim file
        /// </summary>
        /// <param name="providerId"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        Task<bool> StoreSourceFileAsync(int providerId, Stream fileStream);

        /// <summary>
        /// Set repository in temp mode
        /// </summary>
        bool TempMode { get; set; }
    }
}