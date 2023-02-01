using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using RecordParser.Builders.Reader;
using SOS.Harvest.Services.Taxon.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Harvest.Services.Taxon
{
    public class TaxonService : ITaxonService
    {
        private const string DyntaxaTaxonIdPrefix = "urn:lsid:dyntaxa.se:Taxon:";
        private readonly ILogger<TaxonService> _logger;
        private readonly string _taxonDwcUrl;
        private readonly ITaxonServiceProxy _taxonServiceProxy;

      
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="taxonServiceProxy"></param>
        /// <param name="taxonServiceConfiguration"></param>
        /// <param name="logger"></param>
        public TaxonService(
            ITaxonServiceProxy taxonServiceProxy,
            TaxonServiceConfiguration taxonServiceConfiguration,
            ILogger<TaxonService> logger)
        {
            _taxonDwcUrl = taxonServiceConfiguration?.BaseAddress ??
                               throw new ArgumentNullException(nameof(taxonServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _taxonServiceProxy = taxonServiceProxy ?? throw new ArgumentNullException(nameof(taxonServiceProxy));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DarwinCoreTaxon>> GetTaxaAsync()
        {
            try
            {
                _logger.LogInformation("Start fetching dwca file from taxonservice:" + _taxonDwcUrl);
                await using var zipFileContentStream = await _taxonServiceProxy.GetDwcaFileAsync(_taxonDwcUrl);         
                if (zipFileContentStream == null)
                {
                    _logger.LogError("Failed to fetch dwca file from taxon service:" + _taxonDwcUrl);
                    return null!;
                }
                using var zipArchive = new ZipArchive(zipFileContentStream, ZipArchiveMode.Read, false);
                var missingFiles = new[] {
                    "Taxon.csv",
                    "TaxonNameProperties.csv",
                    "TaxonRelations.csv",
                    "TaxonProperties.csv" }
                .Where(f => !zipArchive.Entries.Select(e => e.Name).Contains(f, StringComparer.CurrentCultureIgnoreCase))
                .Select(f => f);

                if (missingFiles.Any())
                {
                    _logger.LogError($"Missing files in Taxon DwC ({string.Join(',', missingFiles)})");
                    return null!;
                }
                    
                var csvFieldDelimiter = GetCsvFieldDelimiterFromMetaFile(zipArchive);
                var taxa = GetTaxonCoreData(zipArchive, csvFieldDelimiter);
                AddVernacularNames(taxa, zipArchive, csvFieldDelimiter);
                AddTaxonRelations(taxa, zipArchive, csvFieldDelimiter);
                AddTaxonProperties(taxa, zipArchive, csvFieldDelimiter);
                return taxa.Values;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            return null!;
        }

        private IVariableLengthReaderBuilder<DarwinCoreTaxon> TaxonMapping => new VariableLengthReaderBuilder<DarwinCoreTaxon>()
        .Map(t => t.TaxonID, indexColumn: 0)
        .Map(t => t.AcceptedNameUsageID, 1)
        .Map(t => t.ParentNameUsageID, 2)
        .Map(t => t.ScientificName, 3)
        .Map(t => t.TaxonRank, 4)
        .Map(t => t.ScientificNameAuthorship, 5)
        .Map(t => t.TaxonomicStatus, 6)
        .Map(t => t.NomenclaturalStatus, 7)
        .Map(t => t.TaxonRemarks, 8)
        .Map(t => t.Kingdom, 9)
        .Map(t => t.Phylum, 10)
        .Map(t => t.Class, 11)
        .Map(t => t.Order, 12)
        .Map(t => t.Family, 13)
        .Map(t => t.Genus, 14);

        private IVariableLengthReaderBuilder<TaxonRelation<string>> TaxonRelationMapping => new VariableLengthReaderBuilder<TaxonRelation<string>>()
            .Map(t => t.ParentTaxonId, indexColumn: 0)
            .Map(t => t.ChildTaxonId, 1)
            .Map(t => t.IsMainRelation, 2);

        private IVariableLengthReaderBuilder<TaxonSortOrder<string>> TaxonSortOrderMapping => new VariableLengthReaderBuilder<TaxonSortOrder<string>>()
            .Map(t => t.TaxonId, indexColumn: 0)
            .Map(t => t.SortOrder, 1);

        private IVariableLengthReaderBuilder<TaxonProperties<string>> TaxonPropertiesMapping => new VariableLengthReaderBuilder<TaxonProperties<string>>()
            .Map(t => t.TaxonId, indexColumn: 0)
            .Map(t => t.SortOrder, 3)
            .Map(t => t.TaxonCategoryId, 4)
            .Map(t => t.TaxonCategorySwedishName, 5)
            .Map(t => t.TaxonCategoryEnglishName, 6)
            .Map(t => t.TaxonCategoryDarwinCoreName, 7)
            .Map(t => t.GbifTaxonId, 10);

        private IVariableLengthReaderBuilder<DarwinCoreVernacularName> VernacularNameMapping =>
            new VariableLengthReaderBuilder<DarwinCoreVernacularName>()
                .Map(t => t.TaxonID, indexColumn: 0)
                .Map(t => t.VernacularName, 1)
                .Map(t => t.Language, 2)
                .Map(t => t.CountryCode, 3)
                .Map(t => t.Source, 4)
                .Map(t => t.IsPreferredName, 5)
                .Map(t => t.TaxonRemarks, 6)
                .Map(t => t.ValidForSighting, 7);



        private string GetCsvFieldDelimiterFromMetaFile(ZipArchive zipArchive)
        {
            var csvFieldDelimiter = "\t";

            // try to get meta data file
            var metaFile = zipArchive.Entries.FirstOrDefault(f =>
                f.Name.Equals("meta.xml", StringComparison.CurrentCultureIgnoreCase));

            // If we found the meta data file, try to get some settings from it
            if (metaFile != null)
            {
                using var reader = XmlReader.Create(metaFile.Open());
                var xmlDoc = XDocument.Load(reader);
                csvFieldDelimiter = xmlDoc.Root?.Element("archive")?.Element("core")?.Attribute("fieldsTerminatedBy")
                                        ?.Value ??
                                    csvFieldDelimiter;
            }

            return csvFieldDelimiter;
        }

        private Dictionary<string, DarwinCoreTaxon> GetTaxonCoreData(ZipArchive zipArchive,
            string csvFieldDelimiter)
        {
            _logger.LogDebug("Start adding taxon core data");
            string strZipEntries = string.Join(", ", zipArchive.Entries.Select(e => e.Name));
            _logger.LogInformation($"The Taxon DwC-A contains the following files: {strZipEntries}");

            // Try to get the taxon data file
            var taxonFile = zipArchive.Entries.FirstOrDefault(f =>
                f.Name.Equals("Taxon.csv", StringComparison.CurrentCultureIgnoreCase));

            if (taxonFile == null)
            {
                _logger.LogError("Failed to open Taxon.csv");
                return null!; // If no taxon data file found, we can't do anything more
            }
            // Read taxon data
            using var csvFileHelper = new CsvFileHelper();
            csvFileHelper.InitializeRead(taxonFile.Open(), csvFieldDelimiter);


        // Get all taxa from file
            var alltaxa = csvFileHelper
                .GetRecords(TaxonMapping);
            csvFileHelper.FinishRead();

            var taxonByTaxonId = alltaxa
                .Where(taxon => taxon.TaxonomicStatus == "accepted")
                .ToDictionary(taxon => taxon.TaxonID, taxon => taxon);

            var synonymsByTaxonId = alltaxa
                .Where(taxon => taxon.TaxonomicStatus != "accepted")
                .GroupBy(taxon => taxon.AcceptedNameUsageID)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.ToArray());

            // Merge synonyms into taxa
            foreach (var taxon in taxonByTaxonId.Values)
            {
                if (synonymsByTaxonId.TryGetValue(taxon.TaxonID, out DarwinCoreTaxon[] synonyms))
                {
                    var synonymeNames = new List<DarwinCoreSynonymName>();
                    
                    foreach (var dwcSynonyme in synonyms)
                    {
                        synonymeNames.Add(CreateDarwinCoreSynonymName(dwcSynonyme));
                    }

                    taxon.Synonyms = synonymeNames;
                }

                taxon.DynamicProperties = new TaxonDynamicProperties();
                taxon.Id = taxon.DynamicProperties.DyntaxaTaxonId = GetTaxonIdfromDyntaxaGuid(taxon.TaxonID);
            }
           
            _logger.LogDebug("Finish adding taxon core data");
            return taxonByTaxonId;
        }

        private static DarwinCoreSynonymName CreateDarwinCoreSynonymName(DarwinCoreTaxon dwcSynonyme)
        {
            DarwinCoreSynonymName synonym = new DarwinCoreSynonymName();
            synonym.ScientificName = dwcSynonyme.ScientificName;
            synonym.NomenclaturalStatus = dwcSynonyme.NomenclaturalStatus;
            synonym.ScientificNameAuthorship = dwcSynonyme.ScientificNameAuthorship;
            synonym.TaxonomicStatus = dwcSynonyme.TaxonomicStatus;
            //synonyme.NameId = synonyme.TaxonID; // probably not needed
            //synonyme.TaxonRemarks = synonyme.TaxonRemarks; // probably not needed
            return synonym;
        }

        private static int GetTaxonIdfromDyntaxaGuid(string dyntaxaTaxonGuid)
        {
            return int.Parse(dyntaxaTaxonGuid.Substring(
                DyntaxaTaxonIdPrefix.Length,
                dyntaxaTaxonGuid.Length - DyntaxaTaxonIdPrefix.Length));
        }

        /// <summary>
        ///     Reads taxon relations from zip file and add parent relations to taxa.
        /// </summary>
        /// <param name="taxa"></param>
        /// <param name="zipArchive"></param>
        /// <param name="csvFieldDelimiter"></param>
        private void AddTaxonRelations(
            Dictionary<string, DarwinCoreTaxon> taxa,
            ZipArchive zipArchive,
            string csvFieldDelimiter)
        {
            _logger.LogDebug("Start adding taxon relations");
            // Try to get TaxonRealtions.csv
            var taxonRelationsFile = zipArchive.Entries.FirstOrDefault(f =>
                f.Name.Equals("TaxonRelations.csv", StringComparison.CurrentCultureIgnoreCase));
            if (taxonRelationsFile == null)
            {
                _logger.LogError("Failed to open TaxonRelations.csv");
                return; // If no taxon relations file found, we can't do anything more
            }

            // Read taxon relations data
            using var csvFileHelper = new CsvFileHelper();
            csvFileHelper.InitializeRead(taxonRelationsFile.Open(), csvFieldDelimiter);

            // Get taxon relations using Guids (string) and convert to taxon relations using Dyntaxa Taxon Id (int)
            var allTaxonRelations = csvFileHelper
                .GetRecords(TaxonRelationMapping)
                .Select(m => new TaxonRelation<int>
                {
                    IsMainRelation = m.IsMainRelation,
                    ParentTaxonId = GetTaxonIdfromDyntaxaGuid(m.ParentTaxonId),
                    ChildTaxonId = GetTaxonIdfromDyntaxaGuid(m.ChildTaxonId)
                });

            csvFileHelper.FinishRead();

            var taxonRelationsByChildId = allTaxonRelations
                .GroupBy(m => m.ChildTaxonId)
                .ToDictionary(g => g.Key, g => g.Select(m => m));

            foreach (var taxon in taxa.Values)
            {
                if (taxonRelationsByChildId.TryGetValue(taxon.DynamicProperties.DyntaxaTaxonId, out var taxonRelations))
                {
                    taxon.DynamicProperties.ParentDyntaxaTaxonId =
                        taxonRelations.FirstOrDefault(m => m.IsMainRelation)?.ParentTaxonId;
                    var secondaryParentDyntaxaTaxonIds = taxonRelations
                        .Where(m => m.IsMainRelation == false)
                        .Select(m => m.ParentTaxonId);

                    if (secondaryParentDyntaxaTaxonIds.Any())
                    {
                        taxon.DynamicProperties.SecondaryParentDyntaxaTaxonIds = secondaryParentDyntaxaTaxonIds;
                    }
                }
            }
            _logger.LogDebug("Finish adding taxon relations");
        }

        /// <summary>
        /// Reads taxon properties from zip file
        /// </summary>
        /// <param name="taxa"></param>
        /// <param name="zipArchive"></param>
        /// <param name="csvFieldDelimiter"></param>
        private void AddTaxonProperties(
            Dictionary<string, DarwinCoreTaxon> taxa,
            ZipArchive zipArchive,
            string csvFieldDelimiter)
        {
            _logger.LogDebug("Start adding taxon properties to taxon");
            // Try to get TaxonProperties.csv
            var taxonPropertiesFile = zipArchive.Entries.FirstOrDefault(f =>
                f.Name.Equals("TaxonProperties.csv", StringComparison.CurrentCultureIgnoreCase));
            
            if (taxonPropertiesFile == null)
            {
                _logger.LogError("Failed to open TaxonProperties.csv, sort order will be set to 0");
                return; // If no taxon properties file found, we can't do anything more
            }
            // Read taxon properties data
            using var csvFileHelper = new CsvFileHelper();
            csvFileHelper.InitializeRead(taxonPropertiesFile.Open(), csvFieldDelimiter);

            // Get taxon properties by guid
            var allTaxonProperties = csvFileHelper
                .GetRecords(TaxonPropertiesMapping)
                .Select(m => new TaxonProperties<int>
                {
                    GbifTaxonId = m.GbifTaxonId,
                    TaxonCategoryId = m.TaxonCategoryId,
                    TaxonCategorySwedishName = m.TaxonCategorySwedishName,
                    TaxonCategoryEnglishName = m.TaxonCategoryEnglishName,
                    TaxonCategoryDarwinCoreName = m.TaxonCategoryDarwinCoreName,
                    SortOrder = m.SortOrder,
                    TaxonId = GetTaxonIdfromDyntaxaGuid(m.TaxonId),                    
                });
            csvFileHelper.FinishRead();
            var taxonPropertiesById = allTaxonProperties
                .ToDictionary(g => g.TaxonId, g => g);

            foreach (var taxon in taxa.Values)
            {
                if (taxonPropertiesById.TryGetValue(taxon.DynamicProperties.DyntaxaTaxonId, out var taxonProperties))
                {
                    taxon.SortOrder = taxonProperties.SortOrder.GetValueOrDefault(0);
                    taxon.DynamicProperties.GbifTaxonId = taxonProperties.GbifTaxonId;
                    taxon.DynamicProperties.TaxonCategoryId = taxonProperties.TaxonCategoryId;
                    taxon.DynamicProperties.TaxonCategorySwedishName = taxonProperties.TaxonCategorySwedishName;
                    taxon.DynamicProperties.TaxonCategoryEnglishName = taxonProperties.TaxonCategoryEnglishName;
                    taxon.DynamicProperties.TaxonCategoryDarwinCoreName = taxonProperties.TaxonCategoryDarwinCoreName;
                }
            }
            _logger.LogDebug("Finish adding taxon properties to taxon");
        }


        /// <summary>
        /// Reads vernacular names from zip file and adds them to taxa.
        /// </summary>
        /// <param name="taxa"></param>
        /// <param name="zipArchive"></param>
        /// <param name="csvFieldDelimiter"></param>
        private void AddVernacularNames(
            Dictionary<string, DarwinCoreTaxon> taxa,
            ZipArchive zipArchive,
            string csvFieldDelimiter)
        {
            _logger.LogDebug("Start adding vernacular names to taxon");
            // Try to get VernacularName.csv
            var vernacularNameFile = zipArchive.Entries.FirstOrDefault(f =>
                f.Name.Equals("TaxonNameProperties.csv", StringComparison.CurrentCultureIgnoreCase));
            if (vernacularNameFile == null)
            {
                _logger.LogError("Failed to open TaxonNameProperties.csv");
                return; // If no vernacular name file found, we can't do anything more
            }
            // Read vernacular name data
            using var csvFileHelper = new CsvFileHelper();
            csvFileHelper.InitializeRead(vernacularNameFile.Open(), csvFieldDelimiter);
           
            // Get all vernacular names from file
            var vernacularNames = csvFileHelper.GetRecords(VernacularNameMapping);
            var vernacularNamesByTaxonId = vernacularNames
                .GroupBy(m => m.TaxonID)
                .ToDictionary(g => g.Key, g => g.Select(m => m));

            csvFileHelper.FinishRead();

            foreach (var taxon in taxa.Values)
            {
                if (vernacularNamesByTaxonId.TryGetValue(taxon.TaxonID, out var dwcVernacularNames))
                {
                    taxon.VernacularNames = dwcVernacularNames;
                    taxon.VernacularName = dwcVernacularNames
                        .FirstOrDefault(m => m.Language == "sv" && m.IsPreferredName)?.VernacularName;
                }
            }
            _logger.LogDebug("Finish adding vernacular names to taxon");
        }

        private class TaxonInfo
        {
            public int SortOrder { get; set; }
            public int Id { get; set; }
        }
    }
}