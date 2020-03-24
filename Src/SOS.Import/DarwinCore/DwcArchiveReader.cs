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
using SOS.Import.Harvesters.Observations;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore
{
    public class DwcArchiveReader : Interfaces.IDwcArchiveReader
    {
        private readonly ILogger<DwcArchiveReader> _logger;

        public DwcArchiveReader(ILogger<DwcArchiveReader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<DwcObservationVerbatim>> ReadArchiveAsync(string archivePath)
        {
            using var archive = new ArchiveReader(archivePath);
            var filename = System.IO.Path.GetFileName(archivePath);
            var verbatimRecords = await GetOccurrenceRecordsAsync(archive, filename);
            await AddEventDataAsync(verbatimRecords, archive);
            return verbatimRecords;
        }

        private async Task<List<DwcObservationVerbatim>> GetOccurrenceRecordsAsync(ArchiveReader archiveReader, string filename)
        {
            IAsyncFileReader occurrenceFileReader = archiveReader.GetAsyncFileReader(RowTypes.Occurrence);
            List<DwcObservationVerbatim> occurrenceRecords = new List<DwcObservationVerbatim>();
            bool dwcIndexSpecified = occurrenceFileReader.FileMetaData.Id.IndexSpecified;

            await foreach (IRow row in occurrenceFileReader.GetDataRowsAsync())
            {
                if (dwcIndexSpecified)
                {
                    var id = row[occurrenceFileReader.FileMetaData.Id.Index]; // todo - should we use the id in some way?
                }

                var verbatimObservation = DwcObservationVerbatimFactory.Create(row, filename);
                occurrenceRecords.Add(verbatimObservation);
            }

            return occurrenceRecords;
        }

        private async Task AddEventDataAsync(List<DwcObservationVerbatim> verbatimRecords, ArchiveReader archiveReader)
        {
            IAsyncFileReader eventFileReader = archiveReader.GetAsyncFileReader(RowTypes.Event);
            if (eventFileReader == null) return;

            Dictionary<string, IEnumerable<DwcObservationVerbatim>> observationsByEventId =
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
                        DwcTermValueMapper.MapValueByTerm(observation, fieldType.Term, val);
                    }
                }
            }
        }

        public bool TryValidateDwcACoreFile(string archivePath, out long nrRows, out string message)
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
