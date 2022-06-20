using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Lib.Jobs.Export
{
    /// <summary>
    ///     Interface for DOI export job
    /// </summary>
    public interface IExportAndStoreJob
    {
        /// <summary>
        ///     Export a file and store it in blob storage
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="blobStorageContainer"></param>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [JobDisplayName("Create a DwC-A file using passed filter and store it in blob storage")]
        [Queue("medium")]
        Task<bool> RunAsync(SearchFilter filter, string blobStorageContainer, string fileName,
            IJobCancellationToken cancellationToken);
    }
}