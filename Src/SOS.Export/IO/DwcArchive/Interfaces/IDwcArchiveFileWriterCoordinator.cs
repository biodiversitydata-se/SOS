using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;

namespace SOS.Export.IO.DwcArchive.Interfaces
{
    public interface IDwcArchiveFileWriterCoordinator
    {
        /// <summary>
        /// Initialize DarwinCore CSV files creation.
        /// </summary>
        void BeginWriteDwcCsvFiles();

        /// <summary>
        /// Write processed observations to DarwinCore CSV file(s) for a specific data provider.
        /// </summary>
        /// <param name="processedObservations"></param>
        /// <param name="dataProvider"></param>
        /// <param name="batchId">If the processing is done in parallel for a data provider, use the batchId to identify tha specifc batch that was processed.</param>
        /// <returns></returns>
        Task<bool> WriteObservations(
            IEnumerable<ProcessedObservation> processedObservations,
            DataProvider dataProvider,
            string batchId = null);

        /// <summary>
        /// Delete created CSV files.
        /// </summary>
        void DeleteCreatedCsvFiles();

        /// <summary>
        /// Create DwC-A for each data provider and DwC-A for all data providers combined.
        /// </summary>
        void CreateDwcaFilesFromCreatedCsvFiles();
    }
}
