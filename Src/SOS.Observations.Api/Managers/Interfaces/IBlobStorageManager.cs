using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.DOI;
using SOS.Lib.Models.Misc;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    ///     DOI manager
    /// </summary>
    public interface IBlobStorageManager
    {
        /// <summary>
        ///     Get file download link
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string GetDOIDownloadUrl(Guid id);

        /// <summary>
        /// Get file download url from blob storage
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        string GetExportDownloadUrl(string fileName);

        /// <summary>
        /// Get export files list
        /// </summary>
        /// <returns></returns>
        IEnumerable<File> GetExportFiles();

        /// <summary>
        ///     Get DOIs
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedResult<DOI>> GetDOIsAsync(int skip, int take);
    }
}