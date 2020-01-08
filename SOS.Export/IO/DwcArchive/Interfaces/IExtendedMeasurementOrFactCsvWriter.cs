using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using SOS.Export.Models;
using SOS.Export.Repositories.Interfaces;

namespace SOS.Export.IO.DwcArchive.Interfaces
{
    public interface IExtendedMeasurementOrFactCsvWriter
    {
        /// <inheritdoc />
        Task<bool> CreateCsvFileAsync(
            Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedDarwinCoreRepository processedDarwinCoreRepository,
            IJobCancellationToken cancellationToken);
    }
}