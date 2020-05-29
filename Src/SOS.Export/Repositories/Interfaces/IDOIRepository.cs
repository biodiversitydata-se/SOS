using System;
using System.Threading.Tasks;
using SOS.Lib.Models.DOI;

namespace SOS.Export.Repositories.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IDOIRepository : IBaseRepository<DOI, Guid>
    {
        /// <summary>
        ///     Add item to collection
        /// </summary>
        /// <param name="doi"></param>
        /// <returns></returns>
        Task<bool> AddAsync(DOI doi);
    }
}