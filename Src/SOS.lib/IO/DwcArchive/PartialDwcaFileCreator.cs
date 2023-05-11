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
    public static class PartialDwcaFileCreator
    {
        /// <summary>
        /// Reads a large DwC-A file and creates a smaller one.
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="outputFolder"></param>
        /// <param name="outputFilename"></param>
        /// <param name="nrRowsLimit"></param>
        /// <param name="startRow"></param>
        public static string CreateDwcaFile(
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
            foreach (var extensionComponent in dwcaFileComponents.Extensions)
            {
                File.WriteAllLines(Path.Join(destinationFolder, extensionComponent.Filename), extensionComponent.GetRowsWithHeader());
            }
            string filePath = FilenameHelper.CreateFilenameWithDate(Path.Join(outputFolder, filename), "zip");
            ZipFile.CreateFromDirectory(destinationFolder, filePath);
            Directory.Delete(destinationFolder, true);
            return filePath;
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
                    dwcaOccurrenceComponent.Rows.Add(line);
                    string occurrenceId = line.Substring(0, Math.Max(line.IndexOf('\t'), 0));
                    dwcaOccurrenceComponent.ObservationIds.Add(occurrenceId);
                    nrObservations++;
                }

                nrRowsRead++;
            }

            return dwcaOccurrenceComponent;
        }

        public static HashSet<string> GetDistinctValuesFromDwcaFile(string sourceFilePath, int nrRowsLimit, int startRow, string term)
        {
            var dwcaFileComponents = new DwcaFileComponents();
            HashSet<string> distinctValues = new HashSet<string>();

            using (FileStream zipToOpen = new FileStream(sourceFilePath, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    var metaEntry = archive.Entries.Single(m => m.FullName.Equals("meta.xml", StringComparison.InvariantCultureIgnoreCase));
                    dwcaFileComponents.Meta = ReadZipEntryAsString(metaEntry);                    
                    var occurrenceEntry = archive.Entries.FirstOrDefault(m => m.FullName.StartsWith("occurrence", StringComparison.InvariantCultureIgnoreCase)
                                                                             || m.FullName.StartsWith("observation", StringComparison.InvariantCultureIgnoreCase));
                    distinctValues = GetDistinctValuesFromOccurrenceCsvFile(nrRowsLimit, startRow, term, occurrenceEntry);                    
                }

                return distinctValues;
            }
        }

        private static HashSet<string> GetDistinctValuesFromOccurrenceCsvFile(int nrRowsLimit, int startRow, string term, ZipArchiveEntry occurrenceEntry)
        {
            int nrRowsRead = 0;
            int nrObservations = 0;
            Stream stream = occurrenceEntry.Open();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            var dwcaOccurrenceComponent = new DwcaOccurrenceComponent() { Filename = occurrenceEntry.Name };
            string[] headers = null;
            Dictionary<string, int> headerIndexByHeader = new Dictionary<string, int>();
            int termIndex = 0;
            HashSet<string> distinctValues = new HashSet<string>();
            while (!reader.EndOfStream && nrObservations < nrRowsLimit)
            {
                string line = reader.ReadLine();
                if (nrRowsRead == 0) // Read header
                {
                    dwcaOccurrenceComponent.Header = line;
                    headers = line.Split('\t');
                    headerIndexByHeader = headers.ToDictionary(h => h, h => Array.IndexOf(headers, h), StringComparer.OrdinalIgnoreCase);
                    termIndex = headerIndexByHeader[term];
                    nrRowsRead++;
                    continue;
                }

                if (startRow < nrRowsRead)
                {
                    string[] values = line.Split('\t');
                    var value = values[termIndex];
                    distinctValues.Add(value);                    
                    nrObservations++;
                }

                nrRowsRead++;
            }

            return distinctValues;
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

                string occurrenceId = line.Substring(0, Math.Max(line.IndexOf('\t'), 0));
                if (observationIds.Contains(occurrenceId))
                {
                    extensionComponent.Rows.Add(line);
                }

                nrRowsRead++;
            }

            return extensionComponent;
        }

        public class DwcaFileComponents
        {
            public string Meta { get; set; }
            public string Eml { get; set; }
            public DwcaOccurrenceComponent OccurrenceComponent { get; set; }
            public List<DwcaExtensionComponent> Extensions { get; set; } = new List<DwcaExtensionComponent>();
        }

        public class DwcaOccurrenceComponent
        {
            public string Filename { get; set; }
            public string Header { get; set; }
            public List<string> Rows { get; set; } = new List<string>();
            public HashSet<string> ObservationIds { get; set; } = new HashSet<string>();
            public List<string> GetRowsWithHeader()
            {
                List<string> rows = new List<string> { Header };
                rows.AddRange(Rows);
                return rows;
            }
        }

        public class DwcaExtensionComponent
        {
            public string Filename { get; set; }
            public string Header { get; set; }
            public List<string> Rows { get; set; } = new List<string>();
            public List<string> GetRowsWithHeader()
            {
                List<string> rows = new List<string> { Header };
                rows.AddRange(Rows);
                return rows;
            }
        }
    }
}
