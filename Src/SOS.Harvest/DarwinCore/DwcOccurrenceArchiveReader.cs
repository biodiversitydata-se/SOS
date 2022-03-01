using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DwC_A;
using DwC_A.Terms;
using Microsoft.Extensions.Logging;
using SOS.Harvest.DarwinCore.Factories;
using SOS.Harvest.DarwinCore.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Harvest.DarwinCore
{
    /// <summary>
    ///     DwC-A reader for occurrence based DwC-A.
    /// </summary>
    public class DwcOccurrenceArchiveReader : IDwcArchiveReaderAsDwcObservation
    {
        private readonly ILogger<DwcArchiveReader> _logger;
        private int _idCounter;
        private int NextId => Interlocked.Increment(ref _idCounter);

        public DwcOccurrenceArchiveReader(ILogger<DwcArchiveReader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idCounter = 0;
        }

        /// <summary>
        ///     Reads a occurrence based DwC-A, and returns observations in batches.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="idIdentifierTuple"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<List<DwcObservationVerbatim>> ReadArchiveInBatchesAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple,
            int batchSize)
        {
            var occurrenceFileReader = archiveReader.GetAsyncCoreFile();
            var idIndex = occurrenceFileReader.GetIdIndex();
            var occurrenceRecords = new List<DwcObservationVerbatim>();

            await foreach (var row in occurrenceFileReader.GetDataRowsAsync())
            {
                var occurrenceRecord = DwcObservationVerbatimFactory.Create(NextId, row, idIdentifierTuple, idIndex);
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
            IIdIdentifierTuple idIdentifierTuple)
        {
            const int batchSize = 100000;
            var observationsBatches = ReadArchiveInBatchesAsync(
                archiveReader,
                idIdentifierTuple,
                batchSize);
            var observations = new List<DwcObservationVerbatim>();
            await foreach (var observationsBatch in observationsBatches)
            {
                observations.AddRange(observationsBatch);
            }

            return observations;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DwcEventOccurrenceVerbatim>> ReadEvents(ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple)
        {
            throw new NotImplementedException("Not implemented for this reader");
        }

        /// <summary>
        ///     Add data from DwC-A extensions.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="occurrenceRecords"></param>
        /// <returns></returns>
        private async Task AddDataFromExtensionsAsync(ArchiveReader archiveReader,
            List<DwcObservationVerbatim> occurrenceRecords)
        {
            await AddEmofExtensionDataAsync(occurrenceRecords, archiveReader);
            await AddMofExtensionDataAsync(occurrenceRecords, archiveReader);
            await AddMultimediaExtensionDataAsync(occurrenceRecords, archiveReader);
            await AddAudubonMediaExtensionDataAsync(occurrenceRecords, archiveReader);
        }

        private async Task AddMultimediaExtensionDataAsync(List<DwcObservationVerbatim> occurrenceRecords,
            ArchiveReader archiveReader)
        {
            try
            {
                var multimediaFileReader = archiveReader.GetAsyncFileReader(RowTypes.Multimedia);
                if (multimediaFileReader == null) return;
                var idIndex = multimediaFileReader.GetIdIndex();
                var observationByRecordId = occurrenceRecords.ToDictionary(v => v.RecordId, v => v);
                await foreach (var row in multimediaFileReader.GetDataRowsAsync())
                {
                    var id = row[idIndex];
                    if (observationByRecordId.TryGetValue(id, out var obs))
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

        private async Task AddAudubonMediaExtensionDataAsync(List<DwcObservationVerbatim> occurrenceRecords,
            ArchiveReader archiveReader)
        {
            try
            {
                var audubonFileReader = archiveReader.GetAsyncFileReader(RowTypes.AudubonMediaDescription);
                if (audubonFileReader == null) return;
                var idIndex = audubonFileReader.GetIdIndex();
                var observationByRecordId = occurrenceRecords.ToDictionary(v => v.RecordId, v => v);
                await foreach (var row in audubonFileReader.GetDataRowsAsync())
                {
                    var id = row[idIndex];
                    if (observationByRecordId.TryGetValue(id, out var obs))
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

        private async Task AddMofExtensionDataAsync(List<DwcObservationVerbatim> occurrenceRecords,
            ArchiveReader archiveReader)
        {
            try
            {
                var mofFileReader = archiveReader.GetAsyncFileReader(RowTypes.MeasurementOrFact);
                if (mofFileReader == null) return;
                var idIndex = mofFileReader.GetIdIndex();
                var observationByRecordId = occurrenceRecords.ToDictionary(v => v.RecordId, v => v);
                await foreach (var row in mofFileReader.GetDataRowsAsync())
                {
                    var id = row[idIndex];
                    if (observationByRecordId.TryGetValue(id, out var obs))
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
        ///     Add Extended Measurement Or Fact data
        /// </summary>
        /// <param name="occurrenceRecords"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddEmofExtensionDataAsync(List<DwcObservationVerbatim> occurrenceRecords,
            ArchiveReader archiveReader)
        {
            try
            {
                var emofFileReader = archiveReader.GetAsyncFileReader(RowTypes.ExtendedMeasurementOrFact);
                if (emofFileReader == null) return;
                var idIndex = emofFileReader.GetIdIndex();

                var observationByRecordId = occurrenceRecords.ToDictionary(v => v.RecordId, v => v);
                await foreach (var row in emofFileReader.GetDataRowsAsync())
                {
                    var id = row[idIndex];
                    if (observationByRecordId.TryGetValue(id, out var obs))
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