using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using SOS.Export.Models;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Search;

namespace SOS.Export.IO.DwcArchive.Interfaces
{
    public interface IDwcArchiveFileWriter
    {
        /// <summary>
        ///     Creates a Darwin Core Archive file.
        ///     The file is stored in a random generated name in the exportPath folder.
        ///     The full path is returned by this function.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="fileName"></param>
        /// <param name="processedObservationRepository">The repository to read observation data from.</param>
        /// <param name="processInfo"></param>
        /// <param name="exportFolderPath">The export folder path where the file will be stored.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel this function.</param>
        /// <returns>The file path to the generated DwC-A file.</returns>
        Task<string> CreateDwcArchiveFileAsync(
            FilterBase filter,
            string fileName,
            IProcessedObservationRepository processedObservationRepository,
            ProcessInfo processInfo,
            string exportFolderPath,
            IJobCancellationToken cancellationToken);

        /// <summary>
        ///     Creates a Darwin Core Archive file where the specified fields in <paramref name="fieldDescriptions" /> are used.
        ///     The file is stored in a random generated name in the exportPath folder.
        ///     The full path is returned by this function.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="fileName"></param>
        /// <param name="processedObservationRepository">The repository to read observation data from.</param>
        /// <param name="fieldDescriptions"></param>
        /// <param name="processInfo"></param>
        /// <param name="exportFolderPath">The export folder path where the file will be stored.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel this function.</param>
        /// <returns>The file path to the generated DwC-A file.</returns>
        Task<string> CreateDwcArchiveFileAsync(
            FilterBase filter,
            string fileName,
            IProcessedObservationRepository processedObservationRepository,
            IEnumerable<FieldDescription> fieldDescriptions,
            ProcessInfo processInfo,
            string exportFolderPath,
            IJobCancellationToken cancellationToken);

        /// <summary>
        /// Write part of DwC-A CSV files to disk.
        /// </summary>
        /// <param name="dwcObservations"></param>
        /// <param name="filePathByFilePart"></param>
        /// <returns></returns>
        Task WriteObservations(
            IEnumerable<DarwinCore> dwcObservations,
            Dictionary<DwcaFilePart, string> filePathByFilePart);

        /// <summary>
        /// Generate DwC-A files for data providers and one with all data providers.
        /// </summary>
        /// <param name="dwcaFileCreationInfo"></param>
        /// <returns></returns>
        Task CreateDwcArchiveFileAsync(DwcaFilesCreationInfo dwcaFileCreationInfo);
    }
}