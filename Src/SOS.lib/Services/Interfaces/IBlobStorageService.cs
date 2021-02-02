using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Misc;

namespace SOS.Lib.Services.Interfaces
{
    /// <summary>
    ///     Interface for blob storage service
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        /// Copy file 
        /// </summary>
        /// <param name="sourceContainer"></param>
        /// <param name="sourceFileName"></param>
        /// <param name="targetContainer"></param>
        /// <param name="targetFileName"></param>
        /// <returns></returns>
        Task<bool> CopyFileAsync(string sourceContainer, string sourceFileName, string targetContainer,
            string targetFileName);

        /// <summary>
        ///     Create container
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<bool> CreateContainerAsync(string name);

        /// <summary>
        ///  Get DOI file download link
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        string GetDOIDownloadUrl(string prefix, string suffix);

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
        Task<IEnumerable<File>> GetExportFilesAsync();

        /// <summary>
        ///     Upload blob to storage
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        Task<bool> UploadBlobAsync(string sourcePath, string container);
    }
}