﻿using DwC_A;
using DwC_A.Terms;
using SOS.Harvest.DarwinCore.Factories;
using SOS.Harvest.DarwinCore.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Harvest.DarwinCore
{
    /// <summary>
    ///     DwC-A reader for occurrence based DwC-A.
    /// </summary>
    public class DwcOccurrenceArchiveReader : IDwcArchiveReaderAsDwcObservation
    {
        private int _idCounter;
        private int NextId => Interlocked.Increment(ref _idCounter);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="idInitValue"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DwcOccurrenceArchiveReader(int idInitValue = 0)
        {
            _idCounter = idInitValue;
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
        public async Task<IEnumerable<DwcEventOccurrenceVerbatim>?> ReadEvents(ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple)
        {
            await Task.Run(() =>
            {
                throw new NotImplementedException("Not implemented for this reader");
            });
            return null; ;
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

        private async Task AddAudubonMediaExtensionDataAsync(List<DwcObservationVerbatim> occurrenceRecords,
            ArchiveReader archiveReader)
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

        private async Task AddMofExtensionDataAsync(List<DwcObservationVerbatim> occurrenceRecords,
            ArchiveReader archiveReader)
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

        /// <summary>
        ///     Add Extended Measurement Or Fact data
        /// </summary>
        /// <param name="occurrenceRecords"></param>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        private async Task AddEmofExtensionDataAsync(List<DwcObservationVerbatim> occurrenceRecords,
            ArchiveReader archiveReader)
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

        /// <summary>
        /// Read data stewardship datasets.
        /// </summary>
        /// <param name="archiveReader"></param>
        /// <returns></returns>
        public async Task<List<DwcVerbatimDataset>?> ReadDatasetsAsync(ArchiveReader archiveReader)
        {
            return await Task.Run(() => null as List<DwcVerbatimDataset>);
        }

        public async Task<List<DwcVerbatimDataset>?> ReadDatasetsAsync(ArchiveReaderContext archiveReaderContext)
        {
            return await Task.Run(() => null as List<DwcVerbatimDataset>);
        }

        public async IAsyncEnumerable<List<DwcObservationVerbatim>?> ReadOccurrencesInBatchesAsync(ArchiveReaderContext archiveReaderContext)
        {
            if (archiveReaderContext == null)
            {
                yield return null;
            }
            var occurrenceFileReader = archiveReaderContext!.ArchiveReader!.GetAsyncCoreFile();
            var idIndex = occurrenceFileReader.GetIdIndex();
            var occurrenceRecords = new List<DwcObservationVerbatim>();

            await foreach (var row in occurrenceFileReader.GetDataRowsAsync())
            {
                var occurrenceRecord = DwcObservationVerbatimFactory.Create(NextId, row, archiveReaderContext.DataProvider, idIndex);
                occurrenceRecords.Add(occurrenceRecord);

                if (occurrenceRecords.Count % archiveReaderContext.BatchSize == 0)
                {
                    await AddDataFromExtensionsAsync(archiveReaderContext.ArchiveReader, occurrenceRecords);
                    yield return occurrenceRecords;
                    occurrenceRecords.Clear();
                }
            }

            await AddDataFromExtensionsAsync(archiveReaderContext.ArchiveReader, occurrenceRecords);
            yield return occurrenceRecords;
        }

        public async Task<IEnumerable<DwcEventOccurrenceVerbatim>?> ReadEvents(ArchiveReaderContext archiveReaderContext)
        {
            return await Task.Run(Array.Empty<DwcEventOccurrenceVerbatim>);
        }
    }
}