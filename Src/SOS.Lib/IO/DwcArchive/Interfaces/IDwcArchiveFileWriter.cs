﻿using Hangfire;
using SOS.Export.Models;
using SOS.Lib.Enums;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SOS.Lib.IO.DwcArchive.Interfaces
{
    public interface IDwcArchiveFileWriter
    {
        /// <summary>
        ///     Creates a Darwin Core Archive file.
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
        Task<FileExportResult> CreateDwcArchiveFileAsync(
            DataProvider dataProvider,
            SearchFilter filter,
            string fileName,
            IProcessedObservationCoreRepository processedObservationRepository,
            ProcessInfo processInfo,
            string exportFolderPath,
            IJobCancellationToken cancellationToken);

        /// <summary>
        ///     Creates a Darwin Core Archive file where the specified fields in <paramref name="fieldDescriptions" /> are used.
        ///     The file is stored in a random generated name in the exportPath folder.
        ///     The full path is returned by this function.
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="filter"></param>
        /// <param name="fileName"></param>
        /// <param name="processedObservationRepository">The repository to read observation data from.</param>
        /// <param name="fieldDescriptions"></param>
        /// <param name="processInfo"></param>
        /// <param name="exportFolderPath">The export folder path where the file will be stored.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel this function.</param>
        /// <returns>The file path to the generated DwC-A file.</returns>
        Task<FileExportResult> CreateDwcArchiveFileAsync(
            DataProvider dataProvider,
            SearchFilter filter,
            string fileName,
            IProcessedObservationCoreRepository processedObservationRepository,
            IEnumerable<FieldDescription> fieldDescriptions,
            ProcessInfo processInfo,
            string exportFolderPath,
            IJobCancellationToken cancellationToken);

        Task<(Stream stream, string filename)> CreateDwcArchiveFileInMemoryAsync(
            DataProvider dataProvider,
            SearchFilter filter,
            IProcessedObservationCoreRepository processedObservationRepository,
            IEnumerable<FieldDescription> fieldDescriptions,
            ProcessInfo processInfo,
            IJobCancellationToken cancellationToken);        

        /// <summary>
        /// Write part of DwC-A CSV files to disk.
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="dwcObservations"></param>
        /// <param name="filePathByFilePart"></param>
        /// <param name="dwcaFilePartsInfo"></param>
        /// <param name="checkForIllegalCharacters"></param>
        /// <returns></returns>
        Task<DwcaWriteResult> WriteHeaderlessDwcaFiles(
            DataProvider dataProvider,
            ICollection<Observation> dwcObservations,
            Dictionary<DwcaFilePart, string> filePathByFilePart,
            DwcaFilePartsInfo dwcaFilePartsInfo,
            bool checkForIllegalCharacters = false);

        /// <summary>
        /// Create a DwC-A file for a data provider.
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="exportFolderPath"></param>
        /// <param name="dwcaFilePartsInfo"></param>
        /// <returns></returns>
        Task<string> CreateDwcArchiveFileAsync(DataProvider dataProvider, string exportFolderPath,
            DwcaFilePartsInfo dwcaFilePartsInfo);

        /// <summary>
        /// Create a DwC-A file for all data providers.
        /// </summary>
        /// <param name="exportFolderPath"></param>
        /// <param name="dwcaFilePartsInfos"></param>
        /// <returns></returns>
        Task<string> CreateCompleteDwcArchiveFileAsync(string exportFolderPath, IEnumerable<DwcaFilePartsInfo> dwcaFilePartsInfos);
    }
}