using SOS.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;

namespace SOS.Lib.IO.DwcArchive
{
    /// <summary>
    /// Provides functionality for creating a small sampling event DwC-A file by extracting events from a large DwC-A file.
    /// The file can then be used in GBIF data validator.    
    /// </summary>
    public static class PartialEventDwcaFileCreator
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
            string filePath = CreateEventDwcaFileFromComponents(dwcaFileComponents, outputFolder, outputFilename);
            return filePath;
        }

        private static string CreateEventDwcaFileFromComponents(EventDwcaFileComponents dwcaFileComponents, string outputFolder, string filename)
        {
            string folderName = Guid.NewGuid().ToString();
            string destinationFolder = Path.Combine(outputFolder, folderName);
            Directory.CreateDirectory(destinationFolder);
            File.WriteAllText(Path.Join(destinationFolder, "meta.xml"), dwcaFileComponents.Meta);
            File.WriteAllText(Path.Join(destinationFolder, "eml.xml"), dwcaFileComponents.Eml);
            File.WriteAllLines(Path.Join(destinationFolder, dwcaFileComponents.EventComponent.Filename), dwcaFileComponents.EventComponent.GetRowsWithHeader());            
            foreach (var extensionComponent in dwcaFileComponents.Extensions)
            {
                File.WriteAllLines(Path.Join(destinationFolder, extensionComponent.Filename), extensionComponent.GetRowsWithHeader());
            }
            string filePath = FilenameHelper.CreateFilenameWithDate(Path.Join(outputFolder, filename), "zip");
            ZipFile.CreateFromDirectory(destinationFolder, filePath);
            Directory.Delete(destinationFolder, true);
            return filePath;
        }

        private static EventDwcaFileComponents GetDwcaFileComponents(string sourceFilePath, int nrRowsLimit, int startRow)
        {
            var dwcaFileComponents = new EventDwcaFileComponents();

            using (FileStream zipToOpen = new FileStream(sourceFilePath, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    var metaEntry = archive.Entries.Single(m => m.FullName.Equals("meta.xml", StringComparison.InvariantCultureIgnoreCase));
                    dwcaFileComponents.Meta = ReadZipEntryAsString(metaEntry);
                    var emlEntry = archive.Entries.Single(m => m.FullName.Equals("eml.xml", StringComparison.InvariantCultureIgnoreCase));
                    dwcaFileComponents.Eml = ReadZipEntryAsString(emlEntry);

                    var eventsEntry = archive.Entries.FirstOrDefault(m => m.FullName.StartsWith("event", StringComparison.InvariantCultureIgnoreCase));
                    string eventsFilename = eventsEntry.Name;
                    var eventComponent = ReadEventCsvFile(nrRowsLimit, startRow, eventsEntry);
                    dwcaFileComponents.EventComponent = eventComponent;

                    var occurrenceEntry = archive.Entries.FirstOrDefault(m => m.FullName.StartsWith("occurrence", StringComparison.InvariantCultureIgnoreCase)
                                                                             || m.FullName.StartsWith("observation", StringComparison.InvariantCultureIgnoreCase));
                    if (occurrenceEntry != null)
                    {
                        var extensionComponent = ReadExtensionCsvFile(occurrenceEntry, eventComponent.EventIds);
                        dwcaFileComponents.Extensions.Add(extensionComponent);
                    }                    

                    var multimediaEntry = archive.Entries.FirstOrDefault(m => m.FullName.StartsWith("multimedia", StringComparison.InvariantCultureIgnoreCase));
                    if (multimediaEntry != null)
                    {
                        var extensionComponent = ReadExtensionCsvFile(multimediaEntry, eventComponent.EventIds);
                        dwcaFileComponents.Extensions.Add(extensionComponent);
                    }

                    var emofEntry = archive.Entries.FirstOrDefault(m => m.FullName.StartsWith("extended", StringComparison.InvariantCultureIgnoreCase)
                                                                     || m.FullName.StartsWith("measurement", StringComparison.InvariantCultureIgnoreCase));
                    if (emofEntry != null)
                    {
                        var extensionComponent = ReadExtensionCsvFile(emofEntry, eventComponent.EventIds);
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

        private static DwcaEventComponent ReadEventCsvFile(int nrRowsLimit, int startRow, ZipArchiveEntry eventsEntry)
        {
            int nrRowsRead = 0;
            int nrObservations = 0;
            Stream stream = eventsEntry.Open();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            var dwcaEventComponent = new DwcaEventComponent() { Filename = eventsEntry.Name };
            while (!reader.EndOfStream && nrObservations < nrRowsLimit)
            {
                string line = reader.ReadLine();
                if (nrRowsRead == 0) // Read header
                {
                    dwcaEventComponent.Header = line;
                    nrRowsRead++;
                    continue;
                }

                if (startRow < nrRowsRead)
                {
                    dwcaEventComponent.Rows.Add(line);
                    string occurrenceId = line.Substring(0, Math.Max(line.IndexOf('\t'), 0));
                    dwcaEventComponent.EventIds.Add(occurrenceId);
                    nrObservations++;
                }

                nrRowsRead++;
            }

            return dwcaEventComponent;
        }

        private static DwcaExtensionComponent ReadExtensionCsvFile(ZipArchiveEntry zipArchiveEntry, HashSet<string> eventIds)
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

                string eventId = line.Substring(0, Math.Max(line.IndexOf('\t'), 0));
                if (eventIds.Contains(eventId))
                {
                    extensionComponent.Rows.Add(line);
                }

                nrRowsRead++;
            }

            return extensionComponent;
        }

        public class EventDwcaFileComponents
        {
            public string Meta { get; set; }
            public string Eml { get; set; }
            public DwcaEventComponent EventComponent { get; set; }            
            public List<DwcaExtensionComponent> Extensions { get; set; } = new List<DwcaExtensionComponent>();
        }

        public class DwcaEventComponent
        {
            public string Filename { get; set; }
            public string Header { get; set; }
            public List<string> Rows { get; set; } = new List<string>();
            public HashSet<string> EventIds { get; set; } = new HashSet<string>();
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
