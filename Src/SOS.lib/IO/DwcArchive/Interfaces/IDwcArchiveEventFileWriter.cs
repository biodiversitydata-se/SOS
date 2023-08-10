using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using SOS.Export.Models;
using SOS.Lib.Enums;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.IO.DwcArchive.Interfaces
{
    public interface IDwcArchiveEventFileWriter
    {
        /// <summary>
        ///  Write part of DwC-A event CSV files to disk.
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="dwcObservations"></param>
        /// <param name="eventFilePathByFilePart"></param>
        /// <param name="writtenEventsData"></param>
        /// <param name="checkForIllegalCharacters"></param>
        /// <returns></returns>
        Task<DwcaBatchWriteResult> WriteHeaderlessEventDwcaFilesAsync(
            DataProvider dataProvider,
            ICollection<Observation> dwcObservations,
            Dictionary<DwcaEventFilePart, string> eventFilePathByFilePart,
            WrittenEventSets writtenEventsData,
            bool checkForIllegalCharacters = false);

        Task<string> CreateEventDwcArchiveFileAsync(
            DataProvider dataProvider,
            string exportFolderPath,
            DwcaFilePartsInfo dwcaFilePartsInfo);

        /// <summary>
        ///     Creates a Darwin Core Archive file where the specified fields in fieldDescriptions are used.
        ///     The file is stored in a random generated name in the exportPath folder.
        ///     The full path is returned by this function.
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="filter"></param>
        /// <param name="fileName"></param>
        /// <param name="processedObservationRepository">The repository to read observation data from.</param>
        /// <param name="processInfo"></param>
        /// <param name="exportFolderPath">The export folder path where the file will be stored.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel this function.</param>
        /// <returns>The file path to the generated DwC-A file.</returns>
        Task<FileExportResult> CreateEventDwcArchiveFileAsync(
            DataProvider dataProvider,
            SearchFilter filter,
            string fileName,
            IProcessedObservationCoreRepository processedObservationRepository,
            ProcessInfo processInfo,
            string exportFolderPath,
            IJobCancellationToken cancellationToken);
    }
}