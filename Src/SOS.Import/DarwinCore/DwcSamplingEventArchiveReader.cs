using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DwC_A;
using DwC_A.Terms;
using Microsoft.Extensions.Logging;
using SOS.Import.DarwinCore.Factories;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore
{
    /// <summary>
    ///     DwC-A reader for sampling event based DwC-A as DwcEvent collection.
    /// </summary>
    public class DwcSamplingEventArchiveReader
    {
        private readonly ILogger<DwcArchiveReader> _logger;

        public DwcSamplingEventArchiveReader(ILogger<DwcArchiveReader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        ///     Reads a sampling event based DwC-A, and returns events in batches.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="idIdentifierTuple"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<List<DwcEvent>> ReadArchiveInBatchesAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple,
            int batchSize)
        {
            var occurrenceFileReader = archiveReader.GetAsyncFileReader(RowTypes.Event);
            var idIndex = occurrenceFileReader.GetIdIndex();
            var eventRecords = new List<DwcEvent>();

            await foreach (var row in occurrenceFileReader.GetDataRowsAsync())
            {
                var eventRecord = DwcEventFactory.Create(row, idIdentifierTuple, idIndex);
                eventRecords.Add(eventRecord);

                if (eventRecords.Count % batchSize == 0)
                {
                    await AddDataFromExtensionsAsync(archiveReader, eventRecords);
                    yield return eventRecords;
                    eventRecords.Clear();
                }
            }

            await AddDataFromExtensionsAsync(archiveReader, eventRecords);
            yield return eventRecords;
        }

        /// <summary>
        ///     Reads a sampling event based DwC-A.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="idIdentifierTuple"></param>
        /// <returns></returns>
        public async Task<List<DwcEvent>> ReadArchiveAsync(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple)
        {
            const int batchSize = 100000;
            var observationsBatches = ReadArchiveInBatchesAsync(
                archiveReader,
                idIdentifierTuple,
                batchSize);
            var dwcEvents = new List<DwcEvent>();
            await foreach (var observationsBatch in observationsBatches)
            {
                dwcEvents.AddRange(observationsBatch);
            }

            return dwcEvents;
        }

        /// <summary>
        ///     Add event data from DwC-A extensions.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="eventRecords"></param>
        /// <returns></returns>
        private async Task AddDataFromExtensionsAsync(ArchiveReader archiveReader, List<DwcEvent> eventRecords)
        {
            await AddEmofExtensionDataAsync(eventRecords, archiveReader);
            await AddMofExtensionDataAsync(eventRecords, archiveReader);
            await AddMultimediaExtensionDataAsync(eventRecords, archiveReader);
            await AddAudubonMediaExtensionDataAsync(eventRecords, archiveReader);
        }

        private async Task AddMofExtensionDataAsync(List<DwcEvent> dwcEvents, ArchiveReader archiveReader)
        {
            var mofFileReader = archiveReader.GetAsyncFileReader(RowTypes.MeasurementOrFact);
            if (mofFileReader == null) return;
            var idIndex = mofFileReader.GetIdIndex();
            var dwcEventByRecordId = dwcEvents.ToDictionary(e => e.RecordId, e => e);

            await foreach (var row in mofFileReader.GetDataRowsAsync())
            {
                var id = row[idIndex];
                if (dwcEventByRecordId.TryGetValue(id, out var dwcEvent))
                {
                    if (dwcEvent.MeasurementOrFacts == null)
                    {
                        dwcEvent.MeasurementOrFacts = new List<DwcMeasurementOrFact>();
                    }

                    var mofItem = DwcMeasurementOrFactFactory.Create(row);
                    dwcEvent.MeasurementOrFacts.Add(mofItem);
                }
            }
        }

        private async Task AddMultimediaExtensionDataAsync(List<DwcEvent> dwcEvents, ArchiveReader archiveReader)
        {
            try
            {
                var multimediaFileReader = archiveReader.GetAsyncFileReader(RowTypes.Multimedia);
                if (multimediaFileReader == null) return;
                var idIndex = multimediaFileReader.GetIdIndex();
                var dwcEventByRecordId = dwcEvents.ToDictionary(e => e.RecordId, e => e);
                await foreach (var row in multimediaFileReader.GetDataRowsAsync())
                {
                    var id = row[idIndex];
                    if (dwcEventByRecordId.TryGetValue(id, out var dwcEvent))
                    {
                        if (dwcEvent.Multimedia == null)
                        {
                            dwcEvent.Multimedia = new List<DwcMultimedia>();
                        }

                        var multimediaItem = DwcMultimediaFactory.Create(row);
                        dwcEvent.Multimedia.Add(multimediaItem);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add Multimedia extension data");
                throw;
            }
        }

        private async Task AddAudubonMediaExtensionDataAsync(List<DwcEvent> dwcEvents, ArchiveReader archiveReader)
        {
            try
            {
                var audubonFileReader = archiveReader.GetAsyncFileReader(RowTypes.AudubonMediaDescription);
                if (audubonFileReader == null) return;
                var idIndex = audubonFileReader.GetIdIndex();
                var dwcEventByRecordId = dwcEvents.ToDictionary(e => e.RecordId, e => e);
                await foreach (var row in audubonFileReader.GetDataRowsAsync())
                {
                    var id = row[idIndex];
                    if (dwcEventByRecordId.TryGetValue(id, out var dwcEvent))
                    {
                        if (dwcEvent.AudubonMedia == null)
                        {
                            dwcEvent.AudubonMedia = new List<DwcAudubonMedia>();
                        }

                        var audubonItem = DwcAudubonMediaFactory.Create(row);
                        dwcEvent.AudubonMedia.Add(audubonItem);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add Audubon media description extension data");
                throw;
            }
        }

        /// <summary>
        ///     Add Extended Measurement Or Fact data to DwcEvent objects.
        /// </summary>
        /// <param name="dwcEvents"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddEmofExtensionDataAsync(List<DwcEvent> dwcEvents, ArchiveReader archiveReader)
        {
            var emofFileReader = archiveReader.GetAsyncFileReader(RowTypes.ExtendedMeasurementOrFact);
            if (emofFileReader == null) return;
            var idIndex = emofFileReader.GetIdIndex();

            var dwcEventByRecordId = dwcEvents.ToDictionary(e => e.RecordId, e => e);
            var occurrenceIdFieldMetaData = emofFileReader.TryGetFieldMetaData(Terms.occurrenceID);

            await foreach (var row in emofFileReader.GetDataRowsAsync())
            {
                if (occurrenceIdFieldMetaData != null)
                {
                    var occurrenceId = row[occurrenceIdFieldMetaData.Index];
                    if (!string.IsNullOrEmpty(occurrenceId)) continue; // skip occurrence measurements.
                }

                var id = row[idIndex];
                if (dwcEventByRecordId.TryGetValue(id, out var dwcEvent))
                {
                    if (dwcEvent.ExtendedMeasurementOrFacts == null)
                    {
                        dwcEvent.ExtendedMeasurementOrFacts = new List<DwcExtendedMeasurementOrFact>();
                    }

                    var emofItem = DwcExtendedMeasurementOrFactFactory.Create(row);
                    dwcEvent.ExtendedMeasurementOrFacts.Add(emofItem);
                }
            }
        }
    }
}