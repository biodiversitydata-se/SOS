using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Import.Repositories.Source.Artportalen.Interfaces
{
    /// <summary>
    ///     Base repository interface
    /// </summary>
    public interface IBaseRepository<T>
    {
        /// <summary>
        ///     Query db
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<IEnumerable<E>> QueryAsync<E>(string query, dynamic parameters);
    }
}