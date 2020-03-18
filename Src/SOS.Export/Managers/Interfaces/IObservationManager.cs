using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using SOS.Export.Models;
using SOS.Lib.Models.Search;

namespace SOS.Export.Managers.Interfaces
{
    /// <summary>
    /// Sighting factory repository
    /// </summary>
    public interface IObservationManager
    {
        /// <summary>
        /// Create a export file and upload it to local storage
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ExportDWCAsync(ExportFilter filter, IJobCancellationToken cancellationToken);

        /// <summary>
        /// Create a export file and use ZendTo to send it to user
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="emailAddress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ExportDWCAsync(ExportFilter filter, string emailAddress, IJobCancellationToken cancellationToken);

        /// <summary>
        /// Export all sightings as DwC-A.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ExportAllAsync(IJobCancellationToken cancellationToken);

        /// <summary>
        /// Export all sightings as DwC-A where only the fields in <paramref name="fieldDescriptions"/> are used, plus the mandatory DwC-A fields.
        /// </summary>
        /// <param name="fieldDescriptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ExportAllAsync(
            IEnumerable<FieldDescription> fieldDescriptions,
            IJobCancellationToken cancellationToken);
    }
}
