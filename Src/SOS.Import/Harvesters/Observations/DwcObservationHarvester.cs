using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DwC_A;
using Hangfire;
using SOS.Import.Harvesters.Observations.DarwinCore;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Observations
{
    public class DwcObservationHarvester : Interfaces.IDwcObservationHarvester
    {
        public async Task<HarvestInfo> HarvestObservationsAsync(string archivePath, IJobCancellationToken cancellationToken)
        {
            //var observations = ReadArchive(archivePath);
            var observations = await ReadArchiveAsync(archivePath);
            return null;
        }

        private async Task<List<DarwinCoreObservationVerbatim>> ReadArchiveAsync(string archivePath)
        {
            using var archive = new ArchiveReader(archivePath);
            var coreFile = archive.GetAsyncCoreFile();
            bool dwcIndexSpecified = coreFile.FileMetaData.Id.IndexSpecified;
            List<DarwinCoreObservationVerbatim> verbatimRecords = new List<DarwinCoreObservationVerbatim>();
            await foreach (IRow row in archive.GetAsyncCoreFile().GetDataRowsAsync())
            {
                DarwinCoreObservationVerbatim verbatimRecord = DarwinCoreObservationVerbatimFactory.Create(row);
                string catalogNumber = null;

                if (dwcIndexSpecified)
                {
                    catalogNumber = row[archive.CoreFile.FileMetaData.Id.Index];
                }
                else
                {
                    catalogNumber = null; //verbatimRecord.CatalogNumber;
                }

                if (string.IsNullOrEmpty(catalogNumber))
                {
                    throw new Exception("Could not parse the catalog number for the verbatimRecord.");
                }

                verbatimRecords.Add(verbatimRecord);
            }

            return verbatimRecords;
        }

        private List<DarwinCoreObservationVerbatim> ReadArchive(string archivePath)
        {
            using var archive = new ArchiveReader(archivePath);
            bool dwcIndexSpecified = archive.CoreFile.FileMetaData.Id.IndexSpecified;
            var dataRows = archive.CoreFile.DataRows;
            List<DarwinCoreObservationVerbatim> verbatimRecords = new List<DarwinCoreObservationVerbatim>();

            foreach (var row in dataRows)
            {
                DarwinCoreObservationVerbatim verbatimRecord = DarwinCoreObservationVerbatimFactory.Create(row);
                string catalogNumber = null;

                if (dwcIndexSpecified)
                {
                    catalogNumber = row[archive.CoreFile.FileMetaData.Id.Index];
                }
                else
                {
                    catalogNumber = null; //verbatimRecord.CatalogNumber;
                }

                if (string.IsNullOrEmpty(catalogNumber))
                {
                    throw new Exception("Could not parse the catalog number for the verbatimRecord.");
                }

                verbatimRecords.Add(verbatimRecord);
            }

            return verbatimRecords;
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
