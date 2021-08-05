using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;

namespace SOS.Export.Managers.Interfaces
{
    /// <summary>
    ///     Observation manager
    /// </summary>
    public interface IObservationManager
    {
        /// <summary>
        ///  Create a export file and use ZendTo to send it to user
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="emailAddress"></param>
        /// <param name="description"></param>
        /// <param name="exportFormat"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ExportAndSendAsync(SearchFilter filter, string emailAddress,
            string description,
            ExportFormat exportFormat,
            IJobCancellationToken cancellationToken);

        /// <summary>
        ///  Export a file and store it in blob storage, 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="blobStorageContainer"></param>
        /// <param name="fileName"></param>
        /// <param name="description"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ExportAndStoreAsync(SearchFilter filter, string blobStorageContainer, string fileName,
            string description,
            IJobCancellationToken cancellationToken);

        /// <summary>
        /// Export a file and store it in blob storage, 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="blobStorageContainer"></param>
        /// <param name="fileName"></param>
        /// <param name="emailAddress"></param>
        /// <param name="description"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ExportAndStoreAsync(SearchFilter filter, string blobStorageContainer, string fileName,
            string emailAddress, string description, IJobCancellationToken cancellationToken);
    }
}