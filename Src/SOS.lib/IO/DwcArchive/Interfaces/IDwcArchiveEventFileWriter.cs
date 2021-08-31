using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using SOS.Export.Models;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.IO.DwcArchive.Interfaces
{
    public interface IDwcArchiveEventFileWriter
    {                         
        /// <summary>
        /// Write part of DwC-A event CSV files to disk.
        /// </summary>
        /// <param name="dwcObservations"></param>
        /// <param name="eventFilePathByFilePart"></param>
        /// <param name="writtenEventsData"></param>
        /// <returns></returns>
        Task WriteHeaderlessEventDwcaFiles(
            ICollection<Observation> dwcObservations,
            Dictionary<DwcaEventFilePart, string> eventFilePathByFilePart,
            WrittenEventSets writtenEventsData);

        Task<string> CreateEventDwcArchiveFileAsync(
            DataProvider dataProvider,
            string exportFolderPath,
            DwcaFilePartsInfo dwcaFilePartsInfo);
    }
}