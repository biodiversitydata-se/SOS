using System.Collections.Generic;
using System.IO;
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
        /// Create a DOI
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> CreateDOIAsync(AdvancedFilter filter, string fileName, IJobCancellationToken cancellationToken);

        /// <summary>
        /// Export all sightings as DwC-A.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ExportAllAsync(string fileName, IJobCancellationToken cancellationToken);

        /// <summary>
        /// Export all sightings as DwC-A where only the fields in <paramref name="fieldDescriptions"/> are used, plus the mandatory DwC-A fields.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fieldDescriptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ExportAllAsync(
            string fileName,
            IEnumerable<FieldDescription> fieldDescriptions,
            IJobCancellationToken cancellationToken);
    }
}
