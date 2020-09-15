using System;
using System.Collections.Generic;
using SOS.Lib.Models.Misc;

namespace SOS.Observations.Services.Interfaces
{
    /// <summary>
    ///     Interface for blob storage service
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        ///     Get DOI file download link
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string GetDOIDownloadUrl(Guid id);

        /// <summary>
        ///     Get DOI file download link
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        string GetExportDownloadUrl(string fileName);

        /// <summary>
        /// List export files
        /// </summary>
        /// <returns></returns>
        IEnumerable<File> GetExportFiles();
    }
}