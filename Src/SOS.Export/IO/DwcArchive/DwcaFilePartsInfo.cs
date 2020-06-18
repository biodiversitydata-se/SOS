using System;
using System.Collections.Generic;
using System.IO;
using SOS.Export.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Export.IO.DwcArchive
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

        public static DwcaFilePartsInfo Create(DataProvider dataProvider, string exportFolderPath)
        {
            var dwcaFilePartsInfo = new DwcaFilePartsInfo
            {
                DataProvider = dataProvider,
                ExportFolder = Path.Combine(exportFolderPath, $"DwcaCreationTempFiles-{dataProvider.Identifier}"),
                FilePathByBatchIdAndFilePart = new Dictionary<string, Dictionary<DwcaFilePart, string>>()
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

        private Dictionary<DwcaFilePart, string> CreateFilePathByFilePart(string batchId)
        {
            string occurrenceCsvFilename = batchId == "" ? "Occurrence.csv" : $"Occurrence-{batchId}.csv";
            string emofCsvFilename = batchId == "" ? "Emof.csv" : $"Emof-{batchId}.csv";
            string multimediaCsvFilename = batchId == "" ? "Multimedia.csv" : $"Multimedia-{batchId}.csv";

            var filePathByFilePart = new Dictionary<DwcaFilePart, string>
            {
                {DwcaFilePart.Occurrence, Path.Combine(ExportFolder,  occurrenceCsvFilename)},
                {DwcaFilePart.Emof, Path.Combine(ExportFolder, emofCsvFilename)},
                {DwcaFilePart.Multimedia, Path.Combine(ExportFolder, multimediaCsvFilename)}
            };

            return filePathByFilePart;
        }
    }
}