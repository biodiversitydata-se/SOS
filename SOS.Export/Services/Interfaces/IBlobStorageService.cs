using System.IO;
using System.Threading.Tasks;

namespace SOS.Export.Services.Interfaces
{
    /// <summary>
    /// Interface for blob storage service
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        /// Create container
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<bool> CreateContainerAsync(string name);

        /// <summary>
        /// Get file stream from storage
        /// </summary>
        /// <param name="container"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        Task<FileStream> GetBlobAsync(string container, string blobName);

        /// <summary>
        /// Upload blob to storage
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        Task<bool> UploadBlobAsync(string sourcePath, string container);
    }
}
