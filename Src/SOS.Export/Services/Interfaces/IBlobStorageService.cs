using System.Threading.Tasks;

namespace SOS.Export.Services.Interfaces
{
    /// <summary>
    ///     Interface for blob storage service
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        ///     Create container
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<bool> CreateContainerAsync(string name);

        /// <summary>
        ///     Upload blob to storage
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        Task<bool> UploadBlobAsync(string sourcePath, string container);
    }
}