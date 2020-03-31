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
    /// DwC-A reader for sampling event based DwC-A as DwcEvent collection.
    /// </summary>
    public class DwcSamplingEventArchiveReader
    {
        private readonly ILogger<DwcArchiveReader> _logger;

        public DwcSamplingEventArchiveReader(ILogger<DwcArchiveReader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Reads a sampling event based DwC-A, and returns events in batches.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<List<DwcEvent>> ReadArchiveInBatchesAsync(
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
                    await AddDataFromExtensionsAsync(archiveReader, eventRecords);
                    yield return eventRecords;
                    eventRecords.Clear();
                }
            }

            await AddDataFromExtensionsAsync(archiveReader, eventRecords);
            yield return eventRecords;
        }

        /// <summary>
        /// Add event data from DwC-A extensions.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <param name="eventRecords"></param>
        /// <returns></returns>
        private async Task AddDataFromExtensionsAsync(ArchiveReader archiveReader, List<DwcEvent> eventRecords)
        {
            await AddEmofExtensionDataAsync(eventRecords, archiveReader);
            await AddMofExtensionDataAsync(eventRecords, archiveReader);
            await AddMultimediaExtensionDataAsync(eventRecords, archiveReader);
        }

        private async Task AddMofExtensionDataAsync(List<DwcEvent> dwcEvents, ArchiveReader archiveReader)
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

        private async Task AddMultimediaExtensionDataAsync(List<DwcEvent> dwcEvents, ArchiveReader archiveReader)
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
        private async Task AddEmofExtensionDataAsync(List<DwcEvent> dwcEvents, ArchiveReader archiveReader)
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