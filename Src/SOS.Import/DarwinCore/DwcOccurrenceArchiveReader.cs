﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DwC_A;
using DwC_A.Terms;
using Microsoft.Extensions.Logging;
using SOS.Import.DarwinCore.Factories;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore
{
    /// <summary>
    /// DwC-A reader for occurrence based DwC-A.
    /// </summary>
    public class DwcOccurrenceArchiveReader : Interfaces.IDwcArchiveReaderAsDwcObservation
    {
        private readonly ILogger<DwcArchiveReader> _logger;

        public DwcOccurrenceArchiveReader(ILogger<DwcArchiveReader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Reads a occurrence based DwC-A, and returns observations in batches.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="batchSize"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<List<DwcObservationVerbatim>> ReadArchiveInBatchesAsDwcObservation(
            ArchiveReader archiveReader, 
            int batchSize,
            string filename)
        {
            IAsyncFileReader occurrenceFileReader = archiveReader.GetAsyncCoreFile();
            int idIndex = occurrenceFileReader.GetIdIndex();
            List<DwcObservationVerbatim> occurrenceRecords = new List<DwcObservationVerbatim>();

            await foreach (IRow row in occurrenceFileReader.GetDataRowsAsync())
            {
                var occurrenceRecord = DwcObservationVerbatimFactory.Create(row, filename, idIndex);
                occurrenceRecords.Add(occurrenceRecord);

                if (occurrenceRecords.Count % batchSize == 0)
                {
                    await AddEmofExtensionDataAsync(occurrenceRecords, archiveReader);
                    await AddMofExtensionDataAsync(occurrenceRecords, archiveReader);
                    yield return occurrenceRecords;
                    occurrenceRecords.Clear();
                }
            }

            await AddEmofExtensionDataAsync(occurrenceRecords, archiveReader);
            await AddMofExtensionDataAsync(occurrenceRecords, archiveReader);
            yield return occurrenceRecords;
        }

        private async Task AddMofExtensionDataAsync(List<DwcObservationVerbatim> occurrenceRecords, ArchiveReader archiveReader)
        {
            try
            {
                IAsyncFileReader mofFileReader = archiveReader.GetAsyncFileReader(RowTypes.MeasurementOrFact);
                if (mofFileReader == null) return;
                int idIndex = mofFileReader.GetIdIndex();
                var observationByRecordId = occurrenceRecords.ToDictionary(v => v.RecordId, v => v);
                await foreach (IRow row in mofFileReader.GetDataRowsAsync())
                {
                    var id = row[idIndex];
                    if (observationByRecordId.TryGetValue(id, out DwcObservationVerbatim obs))
                    {
                        if (obs.ObservationMeasurementOrFacts == null)
                        {
                            obs.ObservationMeasurementOrFacts = new List<DwcMeasurementOrFact>();
                        }

                        var mofItem = DwcMeasurementOrFactFactory.Create(row);
                        obs.ObservationMeasurementOrFacts.Add(mofItem);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add MeasurementOrFact extension data");
                throw;
            }
        }

        /// <summary>
        /// Add Extended Measurement Or Fact data
        /// </summary>
        /// <param name="occurrenceRecords"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddEmofExtensionDataAsync(List<DwcObservationVerbatim> occurrenceRecords, ArchiveReader archiveReader)
        {
            try
            {
                IAsyncFileReader emofFileReader = archiveReader.GetAsyncFileReader(RowTypes.ExtendedMeasurementOrFact);
                if (emofFileReader == null) return;
                int idIndex = emofFileReader.GetIdIndex();

                var observationByRecordId = occurrenceRecords.ToDictionary(v => v.RecordId, v => v);
                await foreach (IRow row in emofFileReader.GetDataRowsAsync())
                {
                    var id = row[idIndex];
                    if (observationByRecordId.TryGetValue(id, out DwcObservationVerbatim obs))
                    {
                        if (obs.ObservationExtendedMeasurementOrFacts == null)
                        {
                            obs.ObservationExtendedMeasurementOrFacts = new List<DwcExtendedMeasurementOrFact>();
                        }

                        var emofItem = DwcExtendedMeasurementOrFactFactory.Create(row);
                        obs.ObservationExtendedMeasurementOrFacts.Add(emofItem);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add ExtendedMeasurementOrFact extension data");
                throw;
            }
        }
    }
}