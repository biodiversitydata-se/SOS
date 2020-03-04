using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using SOS.Export.Models;
using SOS.Lib.Models.Search;

namespace SOS.Export.Factories.Interfaces
{
    /// <summary>
    /// Sighting factory repository
    /// </summary>
    public interface ISightingFactory
    {
        /// <summary>
        /// Create a export file
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> ExportDWCAsync(ExportFilter filter, IJobCancellationToken cancellationToken);

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
