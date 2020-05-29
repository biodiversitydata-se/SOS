using System;
using System.Threading.Tasks;
using SOS.Lib.Models.DOI;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Repositories.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IDOIRepository : IBaseRepository<DOI, Guid>
    {
        /// <summary>
        ///     Get DOIs
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedResult<DOI>> GetDoisAsync(int skip, int take);
    }
}