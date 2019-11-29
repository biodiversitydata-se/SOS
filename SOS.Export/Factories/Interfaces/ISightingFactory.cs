using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using SOS.Export.Models;

namespace SOS.Export.Factories.Interfaces
{
    /// <summary>
    /// Sighting factory repository
    /// </summary>
    public interface ISightingFactory
    {
        /// <summary>
        /// Export all sightings as DwC-A.
        /// </summary>
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
