using SOS.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SOS.Lib.IO.DwcArchive
{
    /// <summary>
    /// Provides functionality for creating a small DwC-A file by extracting observations from a large DwC-A file.
    /// The file can then be used in GBIF data validator.    
    /// </summary>
    public static class DwcaFileValidator
    {
        private static readonly Regex RxIllegalCharacters = new Regex(@"\p{C}+", RegexOptions.Compiled);
        private static readonly Regex RxNewLineTab = new Regex(@"\r\n?|\n|\t", RegexOptions.Compiled);

        /// <summary>
        /// Validates a DwC-A file.
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="outputFolder"></param>
        /// <param name="outputFilename"></param>
        /// <param name="nrRowsLimit"></param>
        /// <param name="startRow"></param>
        /// <returns></returns>
        public static string ValidateFile(
            string sourceFilePath,
            string outputFolder,
            string outputFilename,
            int nrRowsLimit,
            int startRow = 0)
        {
            var dwcaFileComponents = GetDwcaFileComponents(sourceFilePath, nrRowsLimit, startRow);
            string filePath = CreateDwcaFileFromComponents(dwcaFileComponents, outputFolder, outputFilename);
            return filePath;
        }
        
        private static string CreateDwcaFileFromComponents(DwcaFileComponents dwcaFileComponents, string outputFolder, string filename)
        {
            string folderName = Guid.NewGuid().ToString();
            string destinationFolder = Path.Combine(outputFolder, folderName);
            Directory.CreateDirectory(destinationFolder);
            File.WriteAllText(Path.Join(destinationFolder, "meta.xml"), dwcaFileComponents.Meta);
            File.WriteAllText(Path.Join(destinationFolder, "eml.xml"), dwcaFileComponents.Eml);
            File.WriteAllLines(Path.Join(destinationFolder, "occurrence.txt"), dwcaFileComponents.OccurrenceComponent.GetRowsWithHeader());
            File.WriteAllText(Path.Join(destinationFolder, "validation.txt"), GetValidationSummary(dwcaFileComponents));
            foreach (var extensionComponent in dwcaFileComponents.Extensions)
            {
                File.WriteAllLines(Path.Join(destinationFolder, extensionComponent.Filename), extensionComponent.GetRowsWithHeader());
            }
            string filePath = FilenameHelper.CreateFilenameWithDate(Path.Join(outputFolder, filename), "zip");
            ZipFile.CreateFromDirectory(destinationFolder, filePath);
            Directory.Delete(destinationFolder, true);
            return filePath;
        }

        private static string GetValidationSummary(DwcaFileComponents dwcaFileComponents)
        {
            StringBuilder sb = new StringBuilder();
            bool errors = false;

            // Check for duplicates
            var duplicates = dwcaFileComponents.OccurrenceComponent.ObservationIds
                .GroupBy(i => i)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicates.Any())
            {
                errors = true;
                sb.AppendLine("Duplicate observation ids found in occurrence.txt");
                sb.AppendLine("=================================================");
                sb.AppendLine(string.Join(", ", duplicates));
                sb.AppendLine("");
            }

            if (dwcaFileComponents.OccurrenceComponent.ErrorRows.Any())
            {
                errors = true;
                sb.AppendLine("occurrence.txt - error columns");
                sb.AppendLine("==========================================");
                sb.AppendLine(string.Join(", ", dwcaFileComponents.OccurrenceComponent.ErrorColumns));
                sb.AppendLine();

                sb.AppendLine("occurrence.txt - error cells");
                sb.AppendLine("==========================================");
                foreach (var cell in dwcaFileComponents.OccurrenceComponent.ErrorCells)
                {
                    sb.AppendLine($"{cell.Key}=\"{cell.Value}\"");
                }
                sb.AppendLine();

                sb.AppendLine("occurrence.txt - error rows");
                sb.AppendLine("==========================================");
                foreach (var row in dwcaFileComponents.OccurrenceComponent.ErrorRows)
                {
                    sb.AppendLine(row);
                }
                sb.AppendLine("");
            }

            foreach (var extension in dwcaFileComponents.Extensions)
            {
                if (extension.ErrorRows.Any())
                {
                    errors = true;

                    sb.AppendLine($"{extension.Filename} - error columns");
                    sb.AppendLine("==========================================");
                    sb.AppendLine(string.Join(", ", extension.ErrorColumns));
                    sb.AppendLine();

                    sb.AppendLine($"{extension.Filename} - error cells");
                    sb.AppendLine("==========================================");
                    foreach (var cell in extension.ErrorCells)
                    {
                        sb.AppendLine($"{cell.Key}=\"{cell.Value}\"");
                    }
                    sb.AppendLine();

                    sb.AppendLine($"{extension.Filename} - error rows");
                    sb.AppendLine("==========================================");
                    foreach (var row in extension.ErrorRows)
                    {
                        sb.AppendLine(row);
                    }
                    sb.AppendLine("");
                }
            }

            if (!errors)
            {
                sb.AppendLine("Validation OK");
            }

            return sb.ToString();
        }

        private static DwcaFileComponents GetDwcaFileComponents(string sourceFilePath, int nrRowsLimit, int startRow)
        {
            var dwcaFileComponents = new DwcaFileComponents();

            using (FileStream zipToOpen = new FileStream(sourceFilePath, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    var metaEntry = archive.Entries.Single(m => m.FullName.Equals("meta.xml", StringComparison.InvariantCultureIgnoreCase));
                    dwcaFileComponents.Meta = ReadZipEntryAsString(metaEntry);
                    var emlEntry = archive.Entries.Single(m => m.FullName.Equals("eml.xml", StringComparison.InvariantCultureIgnoreCase));
                    dwcaFileComponents.Eml = ReadZipEntryAsString(emlEntry);                    
                    var occurrenceEntry = archive.Entries.FirstOrDefault(m => m.FullName.StartsWith("occurrence", StringComparison.InvariantCultureIgnoreCase)
                                                                             || m.FullName.StartsWith("observation", StringComparison.InvariantCultureIgnoreCase));
                    var occurrenceComponent = ReadOccurrenceCsvFile(nrRowsLimit, startRow, occurrenceEntry);
                    dwcaFileComponents.OccurrenceComponent = occurrenceComponent;

                    var multimediaEntry = archive.Entries.FirstOrDefault(m => m.FullName.StartsWith("multimedia", StringComparison.InvariantCultureIgnoreCase));
                    if (multimediaEntry != null)
                    {
                        var extensionComponent = ReadExtensionCsvFile(multimediaEntry, occurrenceComponent.ObservationIds);
                        dwcaFileComponents.Extensions.Add(extensionComponent);
                    }

                    var emofEntry = archive.Entries.FirstOrDefault(m => m.FullName.StartsWith("extendedmeasurement", StringComparison.InvariantCultureIgnoreCase)
                                                                     || m.FullName.StartsWith("measurement", StringComparison.InvariantCultureIgnoreCase)
                                                                     || m.FullName.StartsWith("emof", StringComparison.InvariantCultureIgnoreCase));
                    if (emofEntry != null)
                    {
                        var extensionComponent = ReadExtensionCsvFile(emofEntry, occurrenceComponent.ObservationIds);
                        dwcaFileComponents.Extensions.Add(extensionComponent);
                    }
                }

                return dwcaFileComponents;
            }
        }

        private static string ReadZipEntryAsString(ZipArchiveEntry zipArchiveEntry)
        {
            Stream stream = zipArchiveEntry.Open();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string str = reader.ReadToEnd();
            return str;
        }

        private static DwcaOccurrenceComponent ReadOccurrenceCsvFile(int nrRowsLimit, int startRow, ZipArchiveEntry occurrenceEntry)
        {
            int nrRowsRead = 0;
            int nrObservations = 0;
            Stream stream = occurrenceEntry.Open();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            var dwcaOccurrenceComponent = new DwcaOccurrenceComponent() { Filename = occurrenceEntry.Name };
            while (!reader.EndOfStream && nrObservations < nrRowsLimit)
            {
                string line = reader.ReadLine();
                if (nrRowsRead == 0) // Read header
                {
                    dwcaOccurrenceComponent.Header = line;
                    nrRowsRead++;
                    continue;
                }

                if (startRow < nrRowsRead)
                {
                    bool containsIllegalCharacters = CheckRowCharacters(line);
                    if (containsIllegalCharacters)
                    {
                        dwcaOccurrenceComponent.ErrorRows.Add(line);
                    }
                    string occurrenceId = line.Substring(0, Math.Max(line.IndexOf('\t'), 0));
                    dwcaOccurrenceComponent.ObservationIds.Add(occurrenceId);
                    nrObservations++;
                }

                nrRowsRead++;
            }

            CalculateErrors(dwcaOccurrenceComponent);
            return dwcaOccurrenceComponent;
        }

        private static bool CheckRowCharacters(string strRow)
        {
            string strCleaned = RxNewLineTab.Replace(strRow, "");
            var match = RxIllegalCharacters.Match(strCleaned);
            bool isMatch = RxIllegalCharacters.IsMatch(strCleaned);
            return isMatch;
        }

        private static void CalculateErrors(DwcaOccurrenceComponent dwcaOccurrenceComponent)
        {
            if (!dwcaOccurrenceComponent.ErrorRows.Any()) return;
            List<string> rows = new List<string> {dwcaOccurrenceComponent.Header};
            rows.AddRange(dwcaOccurrenceComponent.ErrorRows);
            string str = string.Join("\r\n", rows);
            List<Dictionary<string, string>> csvRows = ParseCsv(str);
            foreach (var csvRow in csvRows)
            {
                foreach (var pair in csvRow)
                {
                    if (CheckRowCharacters(pair.Value))
                    {
                        dwcaOccurrenceComponent.ErrorColumns.Add(pair.Key);
                        dwcaOccurrenceComponent.ErrorCells.Add(pair);
                    }
                }
            }
        }

        private static void CalculateErrors(DwcaExtensionComponent dwcaExtensionComponent)
        {
            if (!dwcaExtensionComponent.ErrorRows.Any()) return;
            List<string> rows = new List<string> { dwcaExtensionComponent.Header };
            rows.AddRange(dwcaExtensionComponent.ErrorRows);
            string str = string.Join("\r\n", rows);
            List<Dictionary<string, string>> csvRows = ParseCsv(str);
            foreach (var csvRow in csvRows)
            {
                foreach (var pair in csvRow)
                {
                    if (CheckRowCharacters(pair.Value))
                    {
                        dwcaExtensionComponent.ErrorColumns.Add(pair.Key);
                        dwcaExtensionComponent.ErrorCells.Add(pair);
                    }
                }
            }
        }

        private static DwcaExtensionComponent ReadExtensionCsvFile(ZipArchiveEntry zipArchiveEntry, HashSet<string> observationIds)
        {
            int nrRowsRead = 0;
            Stream stream = zipArchiveEntry.Open();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            var extensionComponent = new DwcaExtensionComponent() { Filename = zipArchiveEntry.Name };
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (nrRowsRead == 0) // Read header
                {
                    extensionComponent.Header = line;
                    nrRowsRead++;
                    continue;
                }

                bool containsIllegalCharacters = CheckRowCharacters(line);
                if (containsIllegalCharacters)
                {
                    extensionComponent.ErrorRows.Add(line);
                }

                nrRowsRead++;
            }

            CalculateErrors(extensionComponent);
            return extensionComponent;
        }

        private static Dictionary<int, string> ParseHeaderByColumnId(string strHeader)
        {
            Dictionary<int, string> headerByColumnId = new Dictionary<int, string>();
            using (var readMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(strHeader ?? "")))
            {
                using (var streamRdr = new StreamReader(readMemoryStream))
                {
                    var csvReader = new NReco.Csv.CsvReader(streamRdr, "\t");
                    
                    // Read header
                    csvReader.Read();
                    for (int i = 0; i < csvReader.FieldsCount; i++)
                    {
                        string val = csvReader[i];
                        headerByColumnId.Add(i, val);
                    }
                }
            }

            return headerByColumnId;
        }

        private static List<Dictionary<string, string>> ParseCsv(string str)
        {
            var items = new List<Dictionary<string, string>>();

            using (var readMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(str ?? "")))
            {
                using (var streamRdr = new StreamReader(readMemoryStream))
                {
                    var csvReader = new NReco.Csv.CsvReader(streamRdr, "\t");
                    var headerByColumnId = new Dictionary<int, string>();

                    // Read header
                    csvReader.Read();
                    for (int i = 0; i < csvReader.FieldsCount; i++)
                    {
                        string val = csvReader[i];
                        headerByColumnId.Add(i, val);
                    }

                    // Read data
                    while (csvReader.Read())
                    {
                        var item = new Dictionary<string, string>();
                        for (int i = 0; i < csvReader.FieldsCount; i++)
                        {
                            string val = csvReader[i];
                            item.Add(headerByColumnId[i], val);
                        }

                        items.Add(item);
                    }
                }

                return items;
            }
        }


        private class DwcaFileComponents
        {
            public string Meta { get; set; }
            public string Eml { get; set; }
            public DwcaOccurrenceComponent OccurrenceComponent { get; set; }
            public List<DwcaExtensionComponent> Extensions { get; set; } = new List<DwcaExtensionComponent>();
        }

        private class DwcaOccurrenceComponent
        {
            public string Filename { get; set; }
            public string Header { get; set; }
            public List<string> ErrorRows { get; set; } = new List<string>();
            public HashSet<string> ErrorColumns { get; set; } = new HashSet<string>();
            public List<KeyValuePair<string, string>> ErrorCells { get; set; } = new List<KeyValuePair<string, string>>();
            public HashSet<string> ObservationIds { get; set; } = new HashSet<string>();
            public List<string> GetRowsWithHeader()
            {
                List<string> rows = new List<string> { Header };
                rows.AddRange(ErrorRows);
                return rows;
            }
        }

        private class DwcaExtensionComponent
        {
            public string Filename { get; set; }
            public string Header { get; set; }
            public List<string> ErrorRows { get; set; } = new List<string>();
            public HashSet<string> ErrorColumns { get; set; } = new HashSet<string>();
            public List<KeyValuePair<string, string>> ErrorCells { get; set; } = new List<KeyValuePair<string, string>>();
            public List<string> GetRowsWithHeader()
            {
                List<string> rows = new List<string> { Header };
                rows.AddRange(ErrorRows);
                return rows;
            }
        }
    }
}
