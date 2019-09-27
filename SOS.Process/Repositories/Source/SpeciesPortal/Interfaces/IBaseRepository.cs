using System.Threading.Tasks;
using System.Collections.Generic;

namespace SOS.Process.Repositories.Source.SpeciesPortal.Interfaces
{
    /// <summary>
    /// Base repository interface
    /// </summary>
    public interface IBaseRepository<T>
    {
        /// <summary>
        /// Query db
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<IEnumerable<E>> QueryAsync<E>(string query, dynamic parameters);

    }
}
