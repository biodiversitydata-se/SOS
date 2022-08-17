using System.Threading.Tasks;
using Hangfire;
using SOS.Export.Models.ZendTo;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search.Filters;

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
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="emailAddress"></param>
        /// <param name="description"></param>
        /// <param name="exportFormat"></param>
        /// <param name="culture"></param>
        /// <param name="flatOut"></param>
        /// <param name="propertyLabelType"></param>
        /// <param name="excludeNullValues"></param>
        /// <param name="sensitiveObservations"></param>
        /// <param name="sendMailFromZendTo"></param>
        /// <param name="encryptPassword"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ZendToResponse> ExportAndSendAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter, 
            string emailAddress,
            string description,
            ExportFormat exportFormat,
            string culture,
            bool flatOut,
            PropertyLabelType propertyLabelType,
            bool excludeNullValues,
            bool sensitiveObservations,
            bool sendMailFromZendTo,
            string encryptPassword,
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