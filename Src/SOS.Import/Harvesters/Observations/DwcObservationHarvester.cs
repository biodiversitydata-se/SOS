using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DwC_A;
using DwC_A.Meta;
using DwC_A.Terms;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.IO;
using SOS.Import.Harvesters.Observations.DarwinCore;
using SOS.Import.Repositories.Destination.DarwinCoreArchive.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Observations
{
    public class DwcObservationHarvester : Interfaces.IDwcObservationHarvester
    {
        private readonly IDarwinCoreArchiveVerbatimRepository _dwcArchiveVerbatimRepository;
        private readonly ILogger<DwcObservationHarvester> _logger;

        public DwcObservationHarvester(
            IDarwinCoreArchiveVerbatimRepository dwcArchiveVerbatimRepository,
            ILogger<DwcObservationHarvester> logger)
        {
            _dwcArchiveVerbatimRepository = dwcArchiveVerbatimRepository;
            _logger = logger;
        }
        
        /// <summary>
        /// Harvest DwC Archive observations
        /// </summary>
        /// <returns></returns>
        public async Task<HarvestInfo> HarvestObservationsAsync(string archivePath, IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(nameof(DarwinCoreObservationVerbatim), DataProvider.Dwc, DateTime.Now);
            try
            {
                _logger.LogDebug("Start storing DwC verbatim");
                var observations = await ReadArchiveAsync(archivePath);
                await _dwcArchiveVerbatimRepository.DeleteCollectionAsync();
                await _dwcArchiveVerbatimRepository.AddCollectionAsync();
                await _dwcArchiveVerbatimRepository.AddManyAsync(observations);
                _logger.LogDebug("Finish storing DwC verbatim");

                cancellationToken?.ThrowIfCancellationRequested();

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = observations?.Count() ?? 0;
            }
            catch (JobAbortedException e)
            {
                _logger.LogError(e, "Canceled harvest of DwC Archive");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed harvest of DwC Archive");
                harvestInfo.Status = RunStatus.Failed;
            }
            return harvestInfo;
        }

        private async Task<List<DarwinCoreObservationVerbatim>> ReadArchiveAsync(string archivePath)
        {
            var sp = Stopwatch.StartNew();
            using var archive = new ArchiveReader(archivePath);
            var filename = System.IO.Path.GetFileName(archivePath).ToString();
            var verbatimRecords = await GetOccurrenceRecordsAsync(archive, filename);
            await AddEventDataAsync(verbatimRecords, archive);
            sp.Stop();
            return verbatimRecords;
        }

        private async Task<List<DarwinCoreObservationVerbatim>> GetOccurrenceRecordsAsync(ArchiveReader archiveReader, string filename)
        {
            IAsyncFileReader occurrenceFileReader = archiveReader.GetAsyncFileReader(RowTypes.Occurrence);
            List<DarwinCoreObservationVerbatim> occurrenceRecords = new List<DarwinCoreObservationVerbatim>();
            bool dwcIndexSpecified = occurrenceFileReader.FileMetaData.Id.IndexSpecified;

            await foreach (IRow row in occurrenceFileReader.GetDataRowsAsync())
            {
                if (dwcIndexSpecified)
                {
                    var id = row[occurrenceFileReader.FileMetaData.Id.Index]; // todo - should we use the id in some way?
                }

                var verbatimObservation = DarwinCoreObservationVerbatimFactory.Create(row, filename);
                occurrenceRecords.Add(verbatimObservation);
            }

            return occurrenceRecords;
        }

        private async Task AddEventDataAsync(List<DarwinCoreObservationVerbatim> verbatimRecords, ArchiveReader archiveReader)
        {
            IAsyncFileReader eventFileReader = archiveReader.GetAsyncFileReader(RowTypes.Event);
            if (eventFileReader == null) return;

            Dictionary<string, IEnumerable<DarwinCoreObservationVerbatim>> observationsByEventId = 
                verbatimRecords
                    .GroupBy(observation => observation.EventID)
                    .ToDictionary(grouping => grouping.Key, grouping => grouping.AsEnumerable());
            bool dwcIndexSpecified = eventFileReader.FileMetaData.Id.IndexSpecified;
            await foreach (IRow row in eventFileReader.GetDataRowsAsync())
            {
                if (dwcIndexSpecified)
                {
                    var id = row[eventFileReader.FileMetaData.Id.Index]; // todo - should we use the id in some way?
                }

                var eventId = row.GetValue(Terms.eventID);
                if (!observationsByEventId.TryGetValue(eventId, out var observations)) continue;
                foreach (var observation in observations)
                {
                    foreach (FieldType fieldType in row.FieldMetaData)
                    {
                        var val = row[fieldType.Index];
                        DwcMapper.MapValueByTerm(observation, fieldType.Term, val);
                    }
                }
            }
        }

        private bool TryValidateDwcACoreFile(string archivePath, out long nrRows, out string message)
        {
            nrRows = 0;
            message = null;

            try
            {
                using var archive = new ArchiveReader(archivePath);
                nrRows = archive.CoreFile.DataRows.LongCount();

                if (archive.CoreFile.FileMetaData.Id.IndexSpecified == false)
                {
                    message = "Core file is missing index of id.";
                    return false;
                }

                if (nrRows == 0)
                {
                    message = "No data rows in core file";
                    return false;
                }
            }
            catch (Exception e)
            {
                message = e.Message;
                return false;
            }

            return true;
        }
    }
}
