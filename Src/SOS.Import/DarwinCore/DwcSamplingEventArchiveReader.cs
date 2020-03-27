using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DwC_A;
using DwC_A.Meta;
using DwC_A.Terms;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Operation.Buffer;
using SOS.Import.DarwinCore.Factories;
using SOS.Import.Harvesters.Observations;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore
{
    /// <summary>
    /// DwC-A reader for sampling event based DwC-A.
    /// </summary>
    public class DwcSamplingEventArchiveReader : Interfaces.IDwcArchiveReaderAsDwcObservation
    {
        private readonly ILogger<DwcArchiveReader> _logger;

        public DwcSamplingEventArchiveReader(ILogger<DwcArchiveReader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Reads a sampling event based DwC-A, and returns observations in batches.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="batchSize"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<List<DwcObservationVerbatim>> ReadArchiveInBatchesAsDwcObservation(
            ArchiveReader archiveReader, int batchSize, string filename)
        {
            IAsyncFileReader occurrenceFileReader = archiveReader.GetAsyncFileReader(RowTypes.Occurrence);
            if (occurrenceFileReader == null) yield break;
            List<DwcObservationVerbatim> occurrenceRecords = new List<DwcObservationVerbatim>();

            await foreach (IRow row in occurrenceFileReader.GetDataRowsAsync())
            {
                var verbatimObservation = DwcObservationVerbatimFactory.Create(row, filename);
                occurrenceRecords.Add(verbatimObservation);

                if (occurrenceRecords.Count % batchSize == 0)
                {
                    await AddEventDataAsync(occurrenceRecords, archiveReader);
                    await AddEmofDataAsync(occurrenceRecords, archiveReader);
                    yield return occurrenceRecords;
                    occurrenceRecords.Clear();
                }
            }

            await AddEventDataAsync(occurrenceRecords, archiveReader);
            await AddEmofDataAsync(occurrenceRecords, archiveReader);
            yield return occurrenceRecords;
        }

        /// <summary>
        /// Add event data to observations by reading the core file.
        /// </summary>
        /// <param name="verbatimRecords"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddEventDataAsync(List<DwcObservationVerbatim> verbatimRecords, ArchiveReader archiveReader)
        {
            IAsyncFileReader eventFileReader = archiveReader.GetAsyncCoreFile();
            Dictionary<string, IEnumerable<DwcObservationVerbatim>> observationsByRecordId = 
                verbatimRecords
                    .GroupBy(observation => observation.RecordId)
                    .ToDictionary(grouping => grouping.Key, grouping => grouping.AsEnumerable());

            await foreach (IRow row in eventFileReader.GetDataRowsAsync())
            {
                var id = row[eventFileReader.FileMetaData.Id.Index];
                if (!observationsByRecordId.TryGetValue(id, out var observations)) continue;
                foreach (var observation in observations)
                {
                    foreach (FieldType fieldType in row.FieldMetaData)
                    {
                        var val = row[fieldType.Index];
                        DwcTermValueMapper.MapValueByTerm(observation, fieldType.Term, val);
                    }
                }
            }
        }

        /// <summary>
        /// Add Extended Measurement Or Fact data
        /// </summary>
        /// <param name="verbatimRecords"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddEmofDataAsync(List<DwcObservationVerbatim> verbatimRecords, ArchiveReader archiveReader)
        {
            try
            {
                IAsyncFileReader emofFileReader = archiveReader.GetAsyncFileReader(RowTypes.ExtendedMeasurementOrFact);
                if (emofFileReader == null) return;
                if (!emofFileReader.FileMetaData.Id.IndexSpecified) return;
                FieldType occurrenceIdFieldMetaData = emofFileReader.TryGetFieldMetaData(Terms.occurrenceID);
                Dictionary<string, IEnumerable<DwcObservationVerbatim>> observationsByRecordId =
                    verbatimRecords
                        .GroupBy(observation => observation.RecordId)
                        .ToDictionary(grouping => grouping.Key, grouping => grouping.AsEnumerable());

                if (occurrenceIdFieldMetaData == null) // If there is no occurrenceID field, then add only event measurements
                {
                    await foreach (IRow row in emofFileReader.GetDataRowsAsync())
                    {
                        string id = row[emofFileReader.FileMetaData.Id.Index];
                        AddEventEmof(row, id, observationsByRecordId);
                    }
                }
                else // occurrenceID field exist, try to get both occurrence measurements and event measurements
                {
                    var observationByOccurrenceId =
                        verbatimRecords.ToDictionary(v => v.OccurrenceID, v => v);
                    await foreach (IRow row in emofFileReader.GetDataRowsAsync())
                    {
                        string occurrenceId = row[occurrenceIdFieldMetaData.Index];
                        AddOccurrenceEmof(row, occurrenceId, observationByOccurrenceId);
                        if (string.IsNullOrEmpty(occurrenceId)) // Measurement for event
                        {
                            string id = row[emofFileReader.FileMetaData.Id.Index];
                            AddEventEmof(row, id, observationsByRecordId);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add ExtendedMeasurementOrFact extension data");
                throw;
            }
        }

        /// <summary>
        /// Add event measurement
        /// </summary>
        /// <param name="row"></param>
        /// <param name="id"></param>
        /// <param name="observationsByRecordId"></param>
        private void AddEventEmof(
            IRow row, 
            string id, 
            Dictionary<string, IEnumerable<DwcObservationVerbatim>> observationsByRecordId)
        {
            if (!observationsByRecordId.TryGetValue(id, out var observations)) return;
            foreach (var observation in observations)
            {
                if (observation.EventExtendedMeasurementOrFacts == null)
                {
                    observation.EventExtendedMeasurementOrFacts = new List<DwcExtendedMeasurementOrFact>();
                }

                var emofItem = DwcExtendedMeasurementOrFactFactory.Create(row);
                observation.EventExtendedMeasurementOrFacts.Add(emofItem);
            }
        }

        /// <summary>
        /// Add occurrence measurement.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="occurrenceId"></param>
        /// <param name="observationByOccurrenceId"></param>
        private void AddOccurrenceEmof(
            IRow row, 
            string occurrenceId, 
            Dictionary<string, DwcObservationVerbatim> observationByOccurrenceId)
        {
            if (string.IsNullOrEmpty(occurrenceId)) return;
            
            if (observationByOccurrenceId.TryGetValue(occurrenceId, out DwcObservationVerbatim obs))
            {
                if (obs.ExtendedMeasurementOrFacts == null)
                {
                    obs.ExtendedMeasurementOrFacts = new List<DwcExtendedMeasurementOrFact>();
                }

                var emofItem = DwcExtendedMeasurementOrFactFactory.Create(row);
                obs.ExtendedMeasurementOrFacts.Add(emofItem);
            }
        }

        /// <summary>
        /// Reads a sampling event based DwC-A, and returns events in batches.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<List<DwcEvent>> ReadEventArchiveInBatchesAsDwcEvent(string archivePath, int batchSize)
        {
            using var archiveReader = new ArchiveReader(archivePath);
            var filename = System.IO.Path.GetFileName(archivePath);
            var occurrenceFileReader = archiveReader.GetAsyncFileReader(RowTypes.Event);
            var eventRecords = new List<DwcEvent>();

            await foreach (IRow row in occurrenceFileReader.GetDataRowsAsync())
            {
                var verbatimEvent = DwcEventFactory.Create(row, filename);
                eventRecords.Add(verbatimEvent);

                if (eventRecords.Count % batchSize == 0)
                {
                    await AddEventEmofDataAsync(eventRecords, archiveReader);
                    yield return eventRecords;
                    eventRecords.Clear();
                }
            }

            await AddEventEmofDataAsync(eventRecords, archiveReader);
            yield return eventRecords;
        }

        /// <summary>
        /// Add Extended Measurement Or Fact data to DwcEvent objects.
        /// </summary>
        /// <param name="dwcEvents"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddEventEmofDataAsync(List<DwcEvent> dwcEvents, ArchiveReader archiveReader)
        {
            IAsyncFileReader emofFileReader = archiveReader.GetAsyncFileReader(RowTypes.ExtendedMeasurementOrFact);
            if (emofFileReader == null) return;
            if (!emofFileReader.FileMetaData.Id.IndexSpecified) return;

            var dwcEventByRecordId = dwcEvents.ToDictionary(e => e.RecordId, e => e);
            FieldType occurrenceIdFieldMetaData = emofFileReader.TryGetFieldMetaData(Terms.occurrenceID);

            await foreach (IRow row in emofFileReader.GetDataRowsAsync())
            {
                if (occurrenceIdFieldMetaData != null)
                {
                    string occurrenceId = row[occurrenceIdFieldMetaData.Index];
                    if (!string.IsNullOrEmpty(occurrenceId)) continue; // skip occurrence measurements.
                }
               
                var id = row[emofFileReader.FileMetaData.Id.Index];
                if (dwcEventByRecordId.TryGetValue(id, out DwcEvent dwcEvent))
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