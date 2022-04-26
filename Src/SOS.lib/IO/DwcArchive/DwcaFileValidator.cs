using SOS.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SOS.Lib.Extensions;
using SOS.Lib.Models.DarwinCore;

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

        public static string Validate(DarwinCore dwcObservation)
        {
            List<string> errors = new List<string>();
            Validate(dwcObservation.Occurrence.OccurrenceID, "OccurrenceID", errors);
            Validate(dwcObservation.Event.EventID, "EventID", errors);
            Validate(dwcObservation.Occurrence.OccurrenceID, "OccurrenceID", errors);
            Validate(dwcObservation.BasisOfRecord, "BasisOfRecord", errors);
            Validate(dwcObservation.BibliographicCitation, "BibliographicCitation", errors);
            Validate(dwcObservation.CollectionCode, "CollectionCode", errors);
            Validate(dwcObservation.CollectionID, "CollectionID", errors);
            Validate(dwcObservation.DataGeneralizations, "DataGeneralizations", errors);
            Validate(dwcObservation.DatasetID, "DatasetID", errors);
            Validate(dwcObservation.DatasetName, "DatasetName", errors);
            Validate(dwcObservation.DynamicProperties, "DynamicProperties", errors);
            Validate(dwcObservation.InformationWithheld, "InformationWithheld", errors);
            Validate(dwcObservation.InstitutionCode, "InstitutionCode", errors);
            Validate(dwcObservation.InstitutionID, "InstitutionID", errors);
            Validate(dwcObservation.Language, "Language", errors);
            Validate(dwcObservation.License, "License", errors);
            Validate(dwcObservation.Modified?.ToString("s", CultureInfo.InvariantCulture), "Modified", errors);
            Validate(dwcObservation.OwnerInstitutionCode, "OwnerInstitutionCode", errors);
            Validate(dwcObservation.References, "References", errors);
            Validate(dwcObservation.RightsHolder, "RightsHolder", errors);
            Validate(dwcObservation.Type, "Type", errors);
            Validate(dwcObservation.Event.Day.HasValue ? dwcObservation.Event.Day.ToString() : null, "Day", errors);
            Validate(dwcObservation.Event.EndDayOfYear.HasValue ? dwcObservation.Event.EndDayOfYear.ToString() : null, "EndDayOfYear", errors);
            Validate(dwcObservation.Event.EventDate, "EventDate", errors);
            Validate(dwcObservation.Event.EventID, "EventID", errors);
            Validate(dwcObservation.Event.EventRemarks, "EventRemarks", errors);
            Validate(dwcObservation.Event.EventTime, "EventTime", errors);
            Validate(dwcObservation.Event.FieldNotes, "FieldNotes", errors);
            Validate(dwcObservation.Event.FieldNumber, "FieldNumber", errors);
            Validate(dwcObservation.Event.Habitat, "Habitat", errors);
            Validate(dwcObservation.Event.Month.HasValue ? dwcObservation.Event.Month.ToString() : null, "Month", errors);
            Validate(dwcObservation.Event.ParentEventID, "ParentEventID", errors);
            Validate(dwcObservation.Event.SampleSizeValue, "SampleSizeValue", errors);
            Validate(dwcObservation.Event.SampleSizeUnit, "SampleSizeUnit", errors);
            Validate(dwcObservation.Event.SamplingEffort, "SamplingEffort", errors);
            Validate(dwcObservation.Event.SamplingProtocol, "SamplingProtocol", errors);
            Validate(dwcObservation.Event.StartDayOfYear.HasValue ? dwcObservation.Event.StartDayOfYear.ToString() : null, "StartDayOfYear", errors);
            Validate(dwcObservation.Event.VerbatimEventDate, "VerbatimEventDate", errors);
            Validate(dwcObservation.Event.Year.HasValue ? dwcObservation.Event.Year.ToString() : null, "Year", errors);
            Validate(dwcObservation.Identification.DateIdentified, "DateIdentified", errors);
            Validate(dwcObservation.Identification.IdentificationID, "IdentificationID", errors);
            Validate(dwcObservation.Identification.IdentificationQualifier, "IdentificationQualifier", errors);
            Validate(dwcObservation.Identification.IdentificationReferences, "IdentificationReferences", errors);
            Validate(dwcObservation.Identification.IdentificationRemarks, "IdentificationRemarks", errors);
            Validate(dwcObservation.Identification.IdentificationVerificationStatus, "IdentificationVerificationStatus", errors);
            Validate(dwcObservation.Identification.IdentifiedBy, "IdentifiedBy", errors);
            Validate(dwcObservation.Identification.TypeStatus, "TypeStatus", errors);
            Validate(dwcObservation.Location.Continent, "Continent", errors);
            Validate(dwcObservation.Location.CoordinatePrecision, "CoordinatePrecision", errors);
            Validate(dwcObservation.Location.CoordinateUncertaintyInMeters.GetValueOrDefault().ToString(), "CoordinateUncertaintyInMeters", errors);
            Validate(dwcObservation.Location.Country, "Country", errors);
            Validate(dwcObservation.Location.CountryCode, "CountryCode", errors);
            Validate(dwcObservation.Location.County, "County", errors);
            Validate(dwcObservation.Location.DecimalLatitude?.ToString("F5", CultureInfo.InvariantCulture), "DecimalLatitude", errors);
            Validate(dwcObservation.Location.DecimalLongitude?.ToString("F5", CultureInfo.InvariantCulture), "DecimalLongitude", errors);
            Validate(dwcObservation.Location.FootprintSpatialFit, "FootprintSpatialFit", errors);
            Validate(dwcObservation.Location.FootprintSRS, "FootprintSRS", errors);
            Validate(dwcObservation.Location.FootprintWKT, "FootprintWKT", errors);
            Validate(dwcObservation.Location.GeodeticDatum, "GeodeticDatum", errors);
            Validate(dwcObservation.Location.GeoreferencedBy, "GeoreferencedBy", errors);
            Validate(dwcObservation.Location.GeoreferencedDate, "GeoreferencedDate", errors);
            Validate(dwcObservation.Location.GeoreferenceProtocol, "GeoreferenceProtocol", errors);
            Validate(dwcObservation.Location.GeoreferenceRemarks, "GeoreferenceRemarks", errors);
            Validate(dwcObservation.Location.GeoreferenceSources, "GeoreferenceSources", errors);
            Validate(dwcObservation.Location.GeoreferenceVerificationStatus, "GeoreferenceVerificationStatus", errors);
            Validate(dwcObservation.Location.HigherGeography, "HigherGeography", errors);
            Validate(dwcObservation.Location.HigherGeographyID, "HigherGeographyID", errors);
            Validate(dwcObservation.Location.Island, "Island", errors);
            Validate(dwcObservation.Location.IslandGroup, "IslandGroup", errors);
            Validate(dwcObservation.Location.Locality, "Locality", errors);
            Validate(dwcObservation.Location.LocationAccordingTo, "LocationAccordingTo", errors);
            Validate(dwcObservation.Location.LocationID, "LocationID", errors);
            Validate(dwcObservation.Location.LocationRemarks, "LocationRemarks", errors);
            Validate(dwcObservation.Location.MaximumDepthInMeters, "MaximumDepthInMeters", errors);
            Validate(dwcObservation.Location.MaximumDistanceAboveSurfaceInMeters, "MaximumDistanceAboveSurfaceInMeters", errors);
            Validate(dwcObservation.Location.MaximumElevationInMeters, "MaximumElevationInMeters", errors);
            Validate(dwcObservation.Location.MinimumDepthInMeters, "MinimumDepthInMeters", errors);
            Validate(dwcObservation.Location.MinimumDistanceAboveSurfaceInMeters, "MinimumDistanceAboveSurfaceInMeters", errors);
            Validate(dwcObservation.Location.MinimumElevationInMeters, "MinimumElevationInMeters", errors);
            Validate(dwcObservation.Location.Municipality, "Municipality", errors);
            Validate(dwcObservation.Location.PointRadiusSpatialFit, "PointRadiusSpatialFit", errors);
            Validate(dwcObservation.Location.StateProvince, "StateProvince", errors);
            Validate(dwcObservation.Location.WaterBody, "WaterBody", errors);
            Validate(dwcObservation.Location.VerbatimCoordinates, "VerbatimCoordinates", errors);
            Validate(dwcObservation.Location.VerbatimCoordinateSystem, "VerbatimCoordinateSystem", errors);
            Validate(dwcObservation.Location.VerbatimDepth, "VerbatimDepth", errors);
            Validate(dwcObservation.Location.VerbatimElevation, "VerbatimElevation", errors);
            Validate(dwcObservation.Location.VerbatimLatitude, "VerbatimLatitude", errors);
            Validate(dwcObservation.Location.VerbatimLocality, "VerbatimLocality", errors);
            Validate(dwcObservation.Location.VerbatimLongitude, "VerbatimLongitude", errors);
            Validate(dwcObservation.Location.VerbatimSRS, "VerbatimSRS", errors);
            Validate(dwcObservation.Occurrence.AssociatedMedia, "AssociatedMedia", errors);
            Validate(dwcObservation.Occurrence.AssociatedReferences, "AssociatedReferences", errors);
            Validate(dwcObservation.Occurrence.AssociatedSequences, "AssociatedSequences", errors);
            Validate(dwcObservation.Occurrence.AssociatedTaxa, "AssociatedTaxa", errors);
            Validate(dwcObservation.Occurrence.Behavior, "Behavior", errors);
            Validate(dwcObservation.Occurrence.CatalogNumber, "CatalogNumber", errors);
            Validate(dwcObservation.Occurrence.Disposition, "Disposition", errors);
            Validate(dwcObservation.Occurrence.EstablishmentMeans, "EstablishmentMeans", errors);
            Validate(dwcObservation.Occurrence.IndividualCount, "IndividualCount", errors);
            Validate(dwcObservation.Occurrence.LifeStage, "LifeStage", errors);
            Validate(dwcObservation.AccessRights, "AccessRights", errors);
            Validate(dwcObservation.Occurrence.OccurrenceRemarks, "OccurrenceRemarks", errors);
            Validate(dwcObservation.Occurrence.OccurrenceStatus, "OccurrenceStatus", errors);
            Validate(dwcObservation.Occurrence.OrganismQuantity, "OrganismQuantity", errors);
            Validate(dwcObservation.Occurrence.OrganismQuantityType, "OrganismQuantityType", errors);
            Validate(dwcObservation.Occurrence.OtherCatalogNumbers, "OtherCatalogNumbers", errors);
            Validate(dwcObservation.Occurrence.Preparations, "Preparations", errors);
            Validate(dwcObservation.Occurrence.RecordedBy, "RecordedBy", errors);
            Validate(dwcObservation.Occurrence.RecordNumber, "RecordNumber", errors);
            Validate(dwcObservation.Occurrence.ReproductiveCondition, "ReproductiveCondition", errors);
            Validate(dwcObservation.Occurrence.Sex, "Sex", errors);
            Validate(dwcObservation.Taxon.AcceptedNameUsage, "AcceptedNameUsage", errors);
            Validate(dwcObservation.Taxon.AcceptedNameUsageID, "AcceptedNameUsageID", errors);
            Validate(dwcObservation.Taxon.Class, "Class", errors);
            Validate(dwcObservation.Taxon.Family, "Family", errors);
            Validate(dwcObservation.Taxon.Genus, "Genus", errors);
            Validate(dwcObservation.Taxon.HigherClassification, "HigherClassification", errors);
            Validate(dwcObservation.Taxon.InfraspecificEpithet, "InfraspecificEpithet", errors);
            Validate(dwcObservation.Taxon.Kingdom, "Kingdom", errors);
            Validate(dwcObservation.Taxon.NameAccordingTo, "NameAccordingTo", errors);
            Validate(dwcObservation.Taxon.NameAccordingToID, "NameAccordingToID", errors);
            Validate(dwcObservation.Taxon.NamePublishedIn, "NamePublishedIn", errors);
            Validate(dwcObservation.Taxon.NamePublishedInID, "NamePublishedInID", errors);
            Validate(dwcObservation.Taxon.NamePublishedInYear, "NamePublishedInYear", errors);
            Validate(dwcObservation.Taxon.NomenclaturalCode, "NomenclaturalCode", errors);
            Validate(dwcObservation.Taxon.NomenclaturalStatus, "NomenclaturalStatus", errors);
            Validate(dwcObservation.Taxon.Order, "Order", errors);
            Validate(dwcObservation.Taxon.OriginalNameUsage, "OriginalNameUsage", errors);
            Validate(dwcObservation.Taxon.OriginalNameUsageID, "OriginalNameUsageID", errors);
            Validate(dwcObservation.Taxon.ParentNameUsage, "ParentNameUsage", errors);
            Validate(dwcObservation.Taxon.ParentNameUsageID, "ParentNameUsageID", errors);
            Validate(dwcObservation.Taxon.Phylum, "Phylum", errors);
            Validate(dwcObservation.Taxon.ScientificName, "ScientificName", errors);
            Validate(dwcObservation.Taxon.ScientificNameAuthorship, "ScientificNameAuthorship", errors);
            Validate(dwcObservation.Taxon.ScientificNameID, "ScientificNameID", errors);
            Validate(dwcObservation.Taxon.SpecificEpithet, "SpecificEpithet", errors);
            Validate(dwcObservation.Taxon.Subgenus, "Subgenus", errors);
            Validate(dwcObservation.Taxon.TaxonConceptID, "TaxonConceptID", errors);
            Validate(dwcObservation.Taxon.TaxonID, "TaxonID", errors);
            Validate(dwcObservation.Taxon.TaxonomicStatus, "TaxonomicStatus", errors);
            Validate(dwcObservation.Taxon.TaxonRank, "TaxonRank", errors);
            Validate(dwcObservation.Taxon.TaxonRemarks, "TaxonRemarks", errors);
            Validate(dwcObservation.Taxon.VerbatimTaxonRank, "VerbatimTaxonRank", errors);
            Validate(dwcObservation.Taxon.VernacularName, "VernacularName", errors);
            Validate(dwcObservation.GeologicalContext?.Bed, "Bed", errors);
            Validate(dwcObservation.GeologicalContext?.EarliestAgeOrLowestStage, "EarliestAgeOrLowestStage", errors);
            Validate(dwcObservation.GeologicalContext?.EarliestEonOrLowestEonothem, "EarliestEonOrLowestEonothem", errors);
            Validate(dwcObservation.GeologicalContext?.EarliestEpochOrLowestSeries, "EarliestEpochOrLowestSeries", errors);
            Validate(dwcObservation.GeologicalContext?.EarliestEraOrLowestErathem, "EarliestEraOrLowestErathem", errors);
            Validate(dwcObservation.GeologicalContext?.EarliestPeriodOrLowestSystem, "EarliestPeriodOrLowestSystem", errors);
            Validate(dwcObservation.GeologicalContext?.Formation, "Formation", errors);
            Validate(dwcObservation.GeologicalContext?.GeologicalContextID, "GeologicalContextID", errors);
            Validate(dwcObservation.GeologicalContext?.Group, "Group", errors);
            Validate(dwcObservation.GeologicalContext?.HighestBiostratigraphicZone, "HighestBiostratigraphicZone", errors);
            Validate(dwcObservation.GeologicalContext?.LatestAgeOrHighestStage, "LatestAgeOrHighestStage", errors);
            Validate(dwcObservation.GeologicalContext?.LatestEonOrHighestEonothem, "LatestEonOrHighestEonothem", errors);
            Validate(dwcObservation.GeologicalContext?.LatestEpochOrHighestSeries, "LatestEpochOrHighestSeries", errors);
            Validate(dwcObservation.GeologicalContext?.LatestEraOrHighestErathem, "LatestEraOrHighestErathem", errors);
            Validate(dwcObservation.GeologicalContext?.LatestPeriodOrHighestSystem, "LatestPeriodOrHighestSystem", errors);
            Validate(dwcObservation.GeologicalContext?.LithostratigraphicTerms, "LithostratigraphicTerms", errors);
            Validate(dwcObservation.GeologicalContext?.LowestBiostratigraphicZone, "LowestBiostratigraphicZone", errors);
            Validate(dwcObservation.GeologicalContext?.Member, "Member", errors);
            Validate(dwcObservation.MaterialSample?.MaterialSampleID, "MaterialSampleID", errors);

            if (errors.Count > 0)
            {
                return $"DwC-A illegal characters in occurrence fields: {string.Join(", ", errors)} [DatasetName={dwcObservation.DatasetName}, OccurrenceID={dwcObservation.Occurrence.OccurrenceID}]";
            }
            return null;
        }


        public static string Validate(ExtendedMeasurementOrFactRow emofRow)
        {
            List<string> errors = new List<string>();
            Validate(emofRow.EventId, "EventId", errors);
            Validate(emofRow.OccurrenceID, "OccurrenceID", errors);
            Validate(emofRow.MeasurementID, "MeasurementID", errors);
            Validate(emofRow.MeasurementType, "MeasurementType", errors);
            Validate(emofRow.MeasurementTypeID, "MeasurementTypeID", errors);
            Validate(emofRow.MeasurementValue, "MeasurementValue", errors);
            Validate(emofRow.MeasurementValueID, "MeasurementValueID", errors);
            Validate(emofRow.MeasurementAccuracy, "MeasurementAccuracy", errors);
            Validate(emofRow.MeasurementUnit, "MeasurementUnit", errors);
            Validate(emofRow.MeasurementUnitID, "MeasurementUnitID", errors);
            Validate(emofRow.MeasurementDeterminedDate, "MeasurementDeterminedDate", errors);
            Validate(emofRow.MeasurementDeterminedBy, "MeasurementDeterminedBy", errors);
            Validate(emofRow.MeasurementRemarks, "MeasurementRemarks", errors);
            Validate(emofRow.MeasurementMethod, "MeasurementMethod", errors);

            if (errors.Count > 0)
            {
                return $"DwC-A illegal characters in emof extension fields: {string.Join(", ", errors)} [OccurrenceID={emofRow.OccurrenceID}]";
            }
            return null;
        }

        public static string Validate(SimpleMultimediaRow multimediaRow)
        {
            List<string> errors = new List<string>();
            Validate(multimediaRow.OccurrenceId, "OccurrenceId", errors);
            Validate(multimediaRow.Type, "Type", errors);
            Validate(multimediaRow.Format, "Format", errors);
            Validate(multimediaRow.Identifier, "Identifier", errors);
            Validate(multimediaRow.References, "References", errors);
            Validate(multimediaRow.Title, "Title", errors);
            Validate(multimediaRow.Description, "Description", errors);
            Validate(multimediaRow.Source, "Source", errors);
            Validate(multimediaRow.Audience, "Audience", errors);
            Validate(multimediaRow.Created, "Created", errors);
            Validate(multimediaRow.Creator, "Creator", errors);
            Validate(multimediaRow.Contributor, "Contributor", errors);
            Validate(multimediaRow.Publisher, "Publisher", errors);
            Validate(multimediaRow.License, "License", errors);
            Validate(multimediaRow.RightsHolder, "RightsHolder", errors);

            if (errors.Count > 0)
            {
                return $"DwC-A illegal characters in multimedia extension fields: {string.Join(", ", errors)} [OccurrenceID={multimediaRow.OccurrenceId}]";
            }
            return null;
        }

        public static void Validate(string value, string field, List<string> errors)
        {
            if (value.ContainsIllegalCharacters())
            {
                errors.Add(field);
            }
        }
    }
}