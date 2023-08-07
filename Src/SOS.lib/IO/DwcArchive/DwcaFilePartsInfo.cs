using System.Collections.Generic;
using System.IO;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.IO.DwcArchive
{
    /// <summary>
    /// Store file paths to all CSV files that should be used to create a 
    /// complete DwC-A file for the data provider.
    /// </summary>
    public class DwcaFilePartsInfo
    {
        public DataProvider DataProvider { get; set; }
        public string ExportFolder { get; set; }
        public Dictionary<string, Dictionary<DwcaFilePart, string>> FilePathByBatchIdAndFilePart { get; set; }
        public Dictionary<string, Dictionary<DwcaEventFilePart, string>> EventFilePathByBatchIdAndFilePart { get; set; }
        public WrittenEventSets WrittenEventsData { get; set; } = new WrittenEventSets();
        public int ObservationCount { get; set; }
        public int ObservationCountBeforeFilter { get; set; }

        public static DwcaFilePartsInfo Create(DataProvider dataProvider, string exportFolderPath)
        {
            var dwcaFilePartsInfo = new DwcaFilePartsInfo
            {
                DataProvider = dataProvider,
                ExportFolder = Path.Combine(exportFolderPath, $"DwcaCreationTempFiles-{dataProvider.Identifier}"),
                FilePathByBatchIdAndFilePart = new Dictionary<string, Dictionary<DwcaFilePart, string>>(),
                EventFilePathByBatchIdAndFilePart = new Dictionary<string, Dictionary<DwcaEventFilePart, string>>()
            };
            return dwcaFilePartsInfo;
        }

        /// <summary>
        /// Get CSV file paths for this data provider and batch id.
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        public Dictionary<DwcaFilePart, string> GetOrCreateFilePathByFilePart(string batchId)
        {
            if (!FilePathByBatchIdAndFilePart.TryGetValue(batchId, out var filePathByFilePart))
            {
                filePathByFilePart = CreateFilePathByFilePart(batchId);
                FilePathByBatchIdAndFilePart.Add(batchId, filePathByFilePart);
            }

            return filePathByFilePart;
        }

        /// <summary>
        /// Get CSV file paths for this data provider and batch id.
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        public Dictionary<DwcaEventFilePart, string> GetOrCreateEventFilePathByFilePart(string batchId)
        {
            if (!EventFilePathByBatchIdAndFilePart.TryGetValue(batchId, out var filePathByFilePart))
            {
                filePathByFilePart = CreateEventFilePathByFilePart(batchId);
                EventFilePathByBatchIdAndFilePart.Add(batchId, filePathByFilePart);
            }

            return filePathByFilePart;
        }

        private Dictionary<DwcaFilePart, string> CreateFilePathByFilePart(string batchId)
        {
            string occurrenceCsvFilename = batchId == "" ? "Occurrence.csv" : $"Occurrence-{batchId}.txt";
            string emofCsvFilename = batchId == "" ? "Emof.csv" : $"Emof-{batchId}.txt";
            string multimediaCsvFilename = batchId == "" ? "Multimedia.csv" : $"Multimedia-{batchId}.txt";

            var filePathByFilePart = new Dictionary<DwcaFilePart, string>
            {
                {DwcaFilePart.Occurrence, Path.Combine(ExportFolder,  occurrenceCsvFilename)},
                {DwcaFilePart.Emof, Path.Combine(ExportFolder, emofCsvFilename)},
                {DwcaFilePart.Multimedia, Path.Combine(ExportFolder, multimediaCsvFilename)}
            };

            return filePathByFilePart;
        }

        private Dictionary<DwcaEventFilePart, string> CreateEventFilePathByFilePart(string batchId)
        {
            string occurrenceCsvFilename = batchId == "" ? "Event-occurrence.csv" : $"Event-occurrence-{batchId}.txt";
            string emofCsvFilename = batchId == "" ? "Event-emof.csv" : $"Event-emof-{batchId}.txt";
            string multimediaCsvFilename = batchId == "" ? "Event-multimedia.csv" : $"Event-multimedia-{batchId}.txt";
            string eventCsvFilename = batchId == "" ? "Event-event.csv" : $"Event-event-{batchId}.txt";

            var eventFilePathByFilePart = new Dictionary<DwcaEventFilePart, string>
            {
                {DwcaEventFilePart.Event, Path.Combine(ExportFolder, eventCsvFilename)},
                {DwcaEventFilePart.Occurrence, Path.Combine(ExportFolder,  occurrenceCsvFilename)},
                {DwcaEventFilePart.Emof, Path.Combine(ExportFolder, emofCsvFilename)},
                {DwcaEventFilePart.Multimedia, Path.Combine(ExportFolder, multimediaCsvFilename)}
            };

            return eventFilePathByFilePart;
        }
    }

    /// <summary>
    /// Class for keeping track of what events has been written to file.
    /// </summary>
    public class WrittenEventSets
    {
        public HashSet<string> WrittenEvents { get; set; } = new HashSet<string>();
        public HashSet<string> WrittenMeasurements { get; set; } = new HashSet<string>();
        public HashSet<string> WrittenMultimedia { get; set; } = new HashSet<string>();
    }
}