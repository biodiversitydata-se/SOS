using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;

namespace SOS.Export.IO.DwcArchive
{
    public class DwcArchiveFileWriterCoordinator : Interfaces.IDwcArchiveFileWriterCoordinator
    {
        private readonly IDwcArchiveFileWriter _dwcArchiveFileWriter;
        private readonly ILogger<DwcArchiveFileWriterCoordinator> _logger;

        public DwcArchiveFileWriterCoordinator(
            IDwcArchiveFileWriter dwcArchiveFileWriter,
            ILogger<DwcArchiveFileWriterCoordinator> logger)
        {
            _dwcArchiveFileWriter = dwcArchiveFileWriter ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initialize DarwinCore CSV files creation.
        /// </summary>
        public void BeginWriteDwcCsvFiles()
        {
            
        }

        /// <summary>
        /// Write processed observations to DarwinCore CSV file(s) for a specific data provider.
        /// </summary>
        /// <param name="processedObservations"></param>
        /// <param name="dataProvider"></param>
        /// <param name="batchId">If the processing is done in parallel for a data provider, use the batchId to identify tha specifc batch that was processed.</param>
        /// <returns></returns>
        public async Task<bool> WriteObservations(
            IEnumerable<ProcessedObservation> processedObservations, 
            DataProvider dataProvider,
            string batchId = null)
        {
            return true;
        }

        public void DeleteCreatedCsvFiles()
        {
            
        }

        /// <summary>
        /// Create DwC-A for each data provider and DwC-A for all data providers combined.
        /// </summary>
        public void CreateDwcaFilesFromCreatedCsvFiles()
        {
            
        }
    }
}
