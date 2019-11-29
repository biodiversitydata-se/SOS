using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using SOS.Export.Models;
using SOS.Export.Repositories.Interfaces;

namespace SOS.Export.IO.DwcArchive.Interfaces
{
    public interface IDwcArchiveOccurrenceCsvWriter
    {
        /// <summary>
        /// Creates a DwC occurrence CSV file.
        /// </summary>
        /// <param name="stream">The stream where the file will be saved.</param>
        /// <param name="fieldDescriptions">The columns that will be used.</param>
        /// <param name="processedDarwinCoreRepository">The repository to read observation data from.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel this function.</param>
        /// <returns>true if the file is written; otherwise false.</returns>
        Task<bool> CreateOccurrenceCsvFileAsync(
            Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedDarwinCoreRepository processedDarwinCoreRepository,
            IJobCancellationToken cancellationToken);
    }
}