using System.Collections.Generic;
using SOS.Lib.Models.Misc;
namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    ///     DOI manager
    /// </summary>
    public interface IBlobStorageManager
    {
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
    }
}