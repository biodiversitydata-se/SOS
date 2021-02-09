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
        /// Set repository in temp mode
        /// </summary>
        bool TempMode { get; set; }
    }
}