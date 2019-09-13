using System.Threading.Tasks;
using System.Collections.Generic;

namespace SOS.Batch.Import.AP.Repositories.Source.Interfaces
{
    /// <summary>
    /// Base repository interface
    /// </summary>
    public interface IBaseRepository<T>
    {
        /// <summary>
        /// Query db
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<E>> QueryAsync<E>(string query);

    }
}
