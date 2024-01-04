﻿using SOS.Lib.Models.Misc;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        Task<IEnumerable<File>> GetExportFilesAsync();
    }
}