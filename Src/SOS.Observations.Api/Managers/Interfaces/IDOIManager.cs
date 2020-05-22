using System;
using System.Threading.Tasks;
using SOS.Lib.Models.DOI;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    /// DOI manager
    /// </summary>
    public interface IDOIManager
    {
        /// <summary>
        /// Get file download link
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string GetDOIDownloadUrl(Guid id);

        /// <summary>
        /// Get DOIs 
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedResult<DOI>> GetDOIsAsync(int skip, int take);
    }
}
