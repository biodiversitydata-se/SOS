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
            ArchiveReader archiveReader, 
            int batchSize, 
            string filename)
        {
            IAsyncFileReader occurrenceFileReader = archiveReader.GetAsyncFileReader(RowTypes.Occurrence);
            if (occurrenceFileReader == null) yield break;
            List<DwcObservationVerbatim> occurrenceRecords = new List<DwcObservationVerbatim>();
            int idIndex = occurrenceFileReader.GetIdIndex();

            await foreach (IRow row in occurrenceFileReader.GetDataRowsAsync())
            {
                var occurrenceRecord = DwcObservationVerbatimFactory.Create(row, filename, idIndex);
                occurrenceRecords.Add(occurrenceRecord);

                if (occurrenceRecords.Count % batchSize == 0)
                {
                    await AddEventDataAsync(occurrenceRecords, archiveReader);
                    await AddEmofExtensionDataAsync(occurrenceRecords, archiveReader);
                    await AddMofExtensionDataAsync(occurrenceRecords, archiveReader);
                    await AddMultimediaExtensionDataAsync(occurrenceRecords, archiveReader);
                    yield return occurrenceRecords;
                    occurrenceRecords.Clear();
                }
            }

            await AddEventDataAsync(occurrenceRecords, archiveReader);
            await AddEmofExtensionDataAsync(occurrenceRecords, archiveReader);
            await AddMofExtensionDataAsync(occurrenceRecords, archiveReader);
            await AddMultimediaExtensionDataAsync(occurrenceRecords, archiveReader);
            yield return occurrenceRecords;
        }

        /// <summary>
        /// Add Measurement Or Fact extension data
        /// </summary>
        /// <param name="occurrenceRecords"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddMofExtensionDataAsync(List<DwcObservationVerbatim> occurrenceRecords, ArchiveReader archiveReader)
        {
            try
            {
                IAsyncFileReader mofFileReader = archiveReader.GetAsyncFileReader(RowTypes.MeasurementOrFact);
                if (mofFileReader == null) return;
                int idIndex = mofFileReader.GetIdIndex();

                Dictionary<string, IEnumerable<DwcObservationVerbatim>> observationsByRecordId =
                    occurrenceRecords
                        .GroupBy(observation => observation.RecordId)
                        .ToDictionary(grouping => grouping.Key, grouping => grouping.AsEnumerable());

                await foreach (IRow row in mofFileReader.GetDataRowsAsync())
                {
                    string id = row[idIndex];
                    AddEventMof(row, id, observationsByRecordId);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add MeasurementOrFact extension data");
                throw;
            }
        }

        /// <summary>
        /// Add Simple Multimedia extension data
        /// </summary>
        /// <param name="occurrenceRecords"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddMultimediaExtensionDataAsync(List<DwcObservationVerbatim> occurrenceRecords, ArchiveReader archiveReader)
        {
            try
            {
                IAsyncFileReader multimediaFileReader = archiveReader.GetAsyncFileReader(RowTypes.Multimedia);
                if (multimediaFileReader == null) return;
                int idIndex = multimediaFileReader.GetIdIndex();
                Dictionary<string, IEnumerable<DwcObservationVerbatim>> observationsByRecordId =
                    occurrenceRecords
                        .GroupBy(observation => observation.RecordId)
                        .ToDictionary(grouping => grouping.Key, grouping => grouping.AsEnumerable());

                await foreach (IRow row in multimediaFileReader.GetDataRowsAsync())
                {
                    string id = row[idIndex];
                    if (!observationsByRecordId.TryGetValue(id, out var observations)) continue;
                    foreach (var observation in observations)
                    {
                        if (observation.EventMultimedia == null)
                        {
                            observation.EventMultimedia = new List<DwcMultimedia>();
                        }

                        var multimediaItem = DwcMultimediaFactory.Create(row);
                        observation.EventMultimedia.Add(multimediaItem);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add Multimedia extension data");
                throw;
            }
        }


        /// <summary>
        /// Add MeasureMentOrFact data
        /// </summary>
        /// <param name="row"></param>
        /// <param name="id"></param>
        /// <param name="observationsByRecordId"></param>
        private void AddEventMof(
            IRow row, 
            string id, 
            Dictionary<string, IEnumerable<DwcObservationVerbatim>> observationsByRecordId)
        {
            if (!observationsByRecordId.TryGetValue(id, out var observations)) return;
            foreach (var observation in observations)
            {
                if (observation.EventMeasurementOrFacts == null)
                {
                    observation.EventMeasurementOrFacts = new List<DwcMeasurementOrFact>();
                }

                var mofItem = DwcMeasurementOrFactFactory.Create(row);
                observation.EventMeasurementOrFacts.Add(mofItem);
            }
        }

        /// <summary>
        /// Add event data to observations by reading the core file.
        /// </summary>
        /// <param name="occurrenceRecords"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddEventDataAsync(List<DwcObservationVerbatim> occurrenceRecords, ArchiveReader archiveReader)
        {
            IAsyncFileReader eventFileReader = archiveReader.GetAsyncCoreFile();
            int idIndex = eventFileReader.GetIdIndex();
            Dictionary<string, IEnumerable<DwcObservationVerbatim>> observationsByRecordId = 
                occurrenceRecords
                    .GroupBy(observation => observation.RecordId)
                    .ToDictionary(grouping => grouping.Key, grouping => grouping.AsEnumerable());

            await foreach (IRow row in eventFileReader.GetDataRowsAsync())
            {
                var id = row[idIndex];
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
                FieldType occurrenceIdFieldMetaData = emofFileReader.TryGetFieldMetaData(Terms.occurrenceID);
                Dictionary<string, IEnumerable<DwcObservationVerbatim>> observationsByRecordId =
                    occurrenceRecords
                        .GroupBy(observation => observation.RecordId)
                        .ToDictionary(grouping => grouping.Key, grouping => grouping.AsEnumerable());

                if (occurrenceIdFieldMetaData == null) // If there is no occurrenceID field, then add only event measurements
                {
                    await foreach (IRow row in emofFileReader.GetDataRowsAsync())
                    {
                        string id = row[idIndex];
                        AddEventEmof(row, id, observationsByRecordId);
                    }
                }
                else // occurrenceID field exist, try to get both occurrence measurements and event measurements
                {
                    var observationByOccurrenceId =
                        occurrenceRecords.ToDictionary(v => v.OccurrenceID, v => v);
                    await foreach (IRow row in emofFileReader.GetDataRowsAsync())
                    {
                        string occurrenceId = row[occurrenceIdFieldMetaData.Index];
                        AddOccurrenceEmof(row, occurrenceId, observationByOccurrenceId);
                        if (string.IsNullOrEmpty(occurrenceId)) // Measurement for event
                        {
                            string id = row[idIndex];
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
                if (obs.ObservationExtendedMeasurementOrFacts == null)
                {
                    obs.ObservationExtendedMeasurementOrFacts = new List<DwcExtendedMeasurementOrFact>();
                }

                var emofItem = DwcExtendedMeasurementOrFactFactory.Create(row);
                obs.ObservationExtendedMeasurementOrFacts.Add(emofItem);
            }
        }

        /// <summary>
        /// Reads a sampling event based DwC-A, and returns events in batches.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<List<DwcEvent>> ReadEventArchiveInBatchesAsDwcEvent(
            ArchiveReader archiveReader, 
            int batchSize)
        {
            var filename = System.IO.Path.GetFileName(archiveReader.FileName);
            var occurrenceFileReader = archiveReader.GetAsyncFileReader(RowTypes.Event);
            int idIndex = occurrenceFileReader.GetIdIndex();
            var eventRecords = new List<DwcEvent>();

            await foreach (IRow row in occurrenceFileReader.GetDataRowsAsync())
            {
                var eventRecord = DwcEventFactory.Create(row, filename, idIndex);
                eventRecords.Add(eventRecord);

                if (eventRecords.Count % batchSize == 0)
                {
                    await AddEventEmofExtensionDataAsync(eventRecords, archiveReader);
                    await AddEventMofExtensionDataAsync(eventRecords, archiveReader);
                    await AddEventMultimediaExtensionDataAsync(eventRecords, archiveReader);
                    yield return eventRecords;
                    eventRecords.Clear();
                }
            }

            await AddEventEmofExtensionDataAsync(eventRecords, archiveReader);
            await AddEventMofExtensionDataAsync(eventRecords, archiveReader);
            await AddEventMultimediaExtensionDataAsync(eventRecords, archiveReader);
            yield return eventRecords;
        }

        private async Task AddEventMofExtensionDataAsync(List<DwcEvent> dwcEvents, ArchiveReader archiveReader)
        {
            IAsyncFileReader mofFileReader = archiveReader.GetAsyncFileReader(RowTypes.MeasurementOrFact);
            if (mofFileReader == null) return;
            int idIndex = mofFileReader.GetIdIndex();
            var dwcEventByRecordId = dwcEvents.ToDictionary(e => e.RecordId, e => e);

            await foreach (IRow row in mofFileReader.GetDataRowsAsync())
            {
                var id = row[idIndex];
                if (dwcEventByRecordId.TryGetValue(id, out DwcEvent dwcEvent))
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

        private async Task AddEventMultimediaExtensionDataAsync(List<DwcEvent> dwcEvents, ArchiveReader archiveReader)
        {
            try
            {
                IAsyncFileReader multimediaFileReader = archiveReader.GetAsyncFileReader(RowTypes.Multimedia);
                if (multimediaFileReader == null) return;
                int idIndex = multimediaFileReader.GetIdIndex();
                var dwcEventByRecordId = dwcEvents.ToDictionary(e => e.RecordId, e => e);
                await foreach (IRow row in multimediaFileReader.GetDataRowsAsync())
                {
                    var id = row[idIndex];
                    if (dwcEventByRecordId.TryGetValue(id, out DwcEvent dwcEvent))
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


        /// <summary>
        /// Add Extended Measurement Or Fact data to DwcEvent objects.
        /// </summary>
        /// <param name="dwcEvents"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddEventEmofExtensionDataAsync(List<DwcEvent> dwcEvents, ArchiveReader archiveReader)
        {
            IAsyncFileReader emofFileReader = archiveReader.GetAsyncFileReader(RowTypes.ExtendedMeasurementOrFact);
            if (emofFileReader == null) return;
            int idIndex = emofFileReader.GetIdIndex();

            var dwcEventByRecordId = dwcEvents.ToDictionary(e => e.RecordId, e => e);
            FieldType occurrenceIdFieldMetaData = emofFileReader.TryGetFieldMetaData(Terms.occurrenceID);

            await foreach (IRow row in emofFileReader.GetDataRowsAsync())
            {
                if (occurrenceIdFieldMetaData != null)
                {
                    string occurrenceId = row[occurrenceIdFieldMetaData.Index];
                    if (!string.IsNullOrEmpty(occurrenceId)) continue; // skip occurrence measurements.
                }
               
                var id = row[idIndex];
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