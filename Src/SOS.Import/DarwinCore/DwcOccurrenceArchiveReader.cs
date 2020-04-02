using System;
using System.Collections.Generic;
using System.IO;
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
        public async IAsyncEnumerable<List<DwcObservationVerbatim>> ReadArchiveInBatchesAsync(
            ArchiveReader archiveReader, 
            int batchSize,
            string filename = null)
        {
            if (filename == null)
            {
                filename = Path.GetFileName(archiveReader.FileName);
            }
            IAsyncFileReader occurrenceFileReader = archiveReader.GetAsyncCoreFile();
            int idIndex = occurrenceFileReader.GetIdIndex();
            List<DwcObservationVerbatim> occurrenceRecords = new List<DwcObservationVerbatim>();

            await foreach (IRow row in occurrenceFileReader.GetDataRowsAsync())
            {
                var occurrenceRecord = DwcObservationVerbatimFactory.Create(row, filename, idIndex);
                occurrenceRecords.Add(occurrenceRecord);

                if (occurrenceRecords.Count % batchSize == 0)
                {
                    await AddDataFromExtensionsAsync(archiveReader, occurrenceRecords);
                    yield return occurrenceRecords;
                    occurrenceRecords.Clear();
                }
            }

            await AddDataFromExtensionsAsync(archiveReader, occurrenceRecords);
            yield return occurrenceRecords;
        }

        public async Task<List<DwcObservationVerbatim>> ReadArchiveAsync(
            ArchiveReader archiveReader,
            string filename = null)
        {
            const int batchSize = 100000;
            if (filename == null)
            {
                filename = Path.GetFileName(archiveReader.FileName);
            }
            var observationsBatches = ReadArchiveInBatchesAsync(
                archiveReader,
                batchSize,
                filename);
            List<DwcObservationVerbatim> observations = new List<DwcObservationVerbatim>();
            await foreach (List<DwcObservationVerbatim> observationsBatch in observationsBatches)
            {
                observations.AddRange(observationsBatch);
            }

            return observations;
        }

        /// <summary>
            /// Add data from DwC-A extensions.
            /// </summary>
            /// <param name="archiveReader"></param>
            /// <param name="occurrenceRecords"></param>
            /// <returns></returns>
            private async Task AddDataFromExtensionsAsync(ArchiveReader archiveReader, List<DwcObservationVerbatim> occurrenceRecords)
        {
            await AddEmofExtensionDataAsync(occurrenceRecords, archiveReader);
            await AddMofExtensionDataAsync(occurrenceRecords, archiveReader);
            await AddMultimediaExtensionDataAsync(occurrenceRecords, archiveReader);
            await AddAudubonMediaExtensionDataAsync(occurrenceRecords, archiveReader);
        }

        private async Task AddMultimediaExtensionDataAsync(List<DwcObservationVerbatim> occurrenceRecords, ArchiveReader archiveReader)
        {
            try
            {
                IAsyncFileReader multimediaFileReader = archiveReader.GetAsyncFileReader(RowTypes.Multimedia);
                if (multimediaFileReader == null) return;
                int idIndex = multimediaFileReader.GetIdIndex();
                var observationByRecordId = occurrenceRecords.ToDictionary(v => v.RecordId, v => v);
                await foreach (IRow row in multimediaFileReader.GetDataRowsAsync())
                {
                    var id = row[idIndex];
                    if (observationByRecordId.TryGetValue(id, out DwcObservationVerbatim obs))
                    {
                        if (obs.ObservationMultimedia == null)
                        {
                            obs.ObservationMultimedia = new List<DwcMultimedia>();
                        }

                        var multimediaItem = DwcMultimediaFactory.Create(row);
                        obs.ObservationMultimedia.Add(multimediaItem);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add Multimedia extension data");
                throw;
            }
        }

        private async Task AddAudubonMediaExtensionDataAsync(List<DwcObservationVerbatim> occurrenceRecords, ArchiveReader archiveReader)
        {
            try
            {
                IAsyncFileReader audubonFileReader = archiveReader.GetAsyncFileReader(RowTypes.AudubonMediaDescription);
                if (audubonFileReader == null) return;
                int idIndex = audubonFileReader.GetIdIndex();
                var observationByRecordId = occurrenceRecords.ToDictionary(v => v.RecordId, v => v);
                await foreach (IRow row in audubonFileReader.GetDataRowsAsync())
                {
                    var id = row[idIndex];
                    if (observationByRecordId.TryGetValue(id, out DwcObservationVerbatim obs))
                    {
                        if (obs.ObservationAudubonMedia == null)
                        {
                            obs.ObservationAudubonMedia = new List<DwcAudubonMedia>();
                        }

                        var audubonItem = DwcAudubonMediaFactory.Create(row);
                        obs.ObservationAudubonMedia.Add(audubonItem);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add Audubon media description extension data");
                throw;
            }
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