using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Process;
using SOS.Process.Services.Interfaces;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Process.Services
{
    public class TaxonService : ITaxonService
    {
        private const string DyntaxaTaxonIdPrefix = "urn:lsid:dyntaxa.se:Taxon:";
        private readonly ILogger<TaxonService> _logger;
        private readonly string _taxonDwcUrl;
        private readonly ITaxonServiceProxy _taxonServiceProxy;

        private static CsvConfiguration GetCsvConfiguration(string csvFieldDelimiter) => new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = csvFieldDelimiter,
            Encoding = Encoding.UTF8,
            HasHeaderRecord = true,
            PrepareHeaderForMatch = (string header, int index) => header.ToLower(),
            HeaderValidated = null,
            MissingFieldFound = null
        };

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
            if (taxonServiceConfiguration.UseTaxonApi)
            {
                _taxonDwcUrl = taxonServiceConfiguration?.TaxonApiAddress ??
                               throw new ArgumentNullException(nameof(taxonServiceConfiguration));
            }
            else
            {
                _taxonDwcUrl = taxonServiceConfiguration?.BaseAddress ??
                               throw new ArgumentNullException(nameof(taxonServiceConfiguration));
            }
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _taxonServiceProxy = taxonServiceProxy ?? throw new ArgumentNullException(nameof(taxonServiceProxy));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DarwinCoreTaxon>> GetTaxaAsync()
        {
            try
            {
                await using var zipFileContentStream = await _taxonServiceProxy.GetDwcaFileAsync(_taxonDwcUrl);
                if (zipFileContentStream == null) return null;
                using var zipArchive = new ZipArchive(zipFileContentStream, ZipArchiveMode.Read, false);
                var csvFieldDelimiter = GetCsvFieldDelimiterFromMetaFile(zipArchive);
                var taxa = GetTaxonCoreData(zipArchive, csvFieldDelimiter);
                AddVernacularNames(taxa, zipArchive, csvFieldDelimiter);
                AddTaxonRelations(taxa, zipArchive, csvFieldDelimiter);
                AddTaxonSortOrders(taxa, zipArchive, csvFieldDelimiter);
                return taxa.Values;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            return null;
        }
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

        private static Dictionary<string, DarwinCoreTaxon> GetTaxonCoreData(ZipArchive zipArchive,
            string csvFieldDelimiter)
        {
            // Try to get the taxon data file
            var taxonFile = zipArchive.Entries.FirstOrDefault(f =>
                f.Name.Equals("Taxon.csv", StringComparison.CurrentCultureIgnoreCase));

            if (taxonFile == null) return null; // If no taxon data file found, we can't do anything more

            // Read taxon data
            using var taxonReader = new StreamReader(taxonFile.Open(), Encoding.UTF8);
            using var taxonCsv = new CsvReader(taxonReader, GetCsvConfiguration(csvFieldDelimiter));

            // Get all taxa from file
            var alltaxa = taxonCsv
                .GetRecords<DarwinCoreTaxon>().ToArray();
            
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
                    var synonymeNames = new List<DarwinCoreSynonymeName>();
                    
                    foreach (var dwcSynonyme in synonyms)
                    {
                        synonymeNames.Add(CreateDarwinCoreSynonymeName(dwcSynonyme));
                    }

                    taxon.Synonyms = synonymeNames;
                }
            }

            foreach (var taxon in taxonByTaxonId.Values)
            {
                taxon.DynamicProperties = new TaxonDynamicProperties();
                taxon.Id = taxon.DynamicProperties.DyntaxaTaxonId = GetTaxonIdfromDyntaxaGuid(taxon.TaxonID);
            }

            return taxonByTaxonId;
        }

        private static DarwinCoreSynonymeName CreateDarwinCoreSynonymeName(DarwinCoreTaxon dwcSynonyme)
        {
            DarwinCoreSynonymeName synonyme = new DarwinCoreSynonymeName();
            synonyme.ScientificName = dwcSynonyme.ScientificName;
            synonyme.NomenclaturalStatus = dwcSynonyme.NomenclaturalStatus;
            synonyme.ScientificNameAuthorship = dwcSynonyme.ScientificNameAuthorship;
            synonyme.TaxonomicStatus = dwcSynonyme.TaxonomicStatus;
            //synonyme.NameId = synonyme.TaxonID; // probably not needed
            //synonyme.TaxonRemarks = synonyme.TaxonRemarks; // probably not needed
            return synonyme;
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
            // Try to get TaxonRealtions.csv
            var taxonRelationsFile = zipArchive.Entries.FirstOrDefault(f =>
                f.Name.Equals("TaxonRelations.csv", StringComparison.CurrentCultureIgnoreCase));
            if (taxonRelationsFile == null) return; // If no taxon relations file found, we can't do anything more

            // Read taxon relations data
            using var taxonRelationsReader = new StreamReader(taxonRelationsFile.Open(), Encoding.UTF8);
            using var taxonRelationsCsv = new CsvReader(taxonRelationsReader, GetCsvConfiguration(csvFieldDelimiter));

            // Get taxon relations using Guids (string) and convert to taxon relations using Dyntaxa Taxon Id (int)
            var allTaxonRelations = taxonRelationsCsv
                .GetRecords<TaxonRelation<string>>()
                .Select(m => new TaxonRelation<int>
                {
                    IsMainRelation = m.IsMainRelation,
                    ParentTaxonId = GetTaxonIdfromDyntaxaGuid(m.ParentTaxonId),
                    ChildTaxonId = GetTaxonIdfromDyntaxaGuid(m.ChildTaxonId)
                });

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
        }

        /// <summary>
        /// Reads taxon sort orders from zip file
        /// </summary>
        /// <param name="taxa"></param>
        /// <param name="zipArchive"></param>
        /// <param name="csvFieldDelimiter"></param>
        private void AddTaxonSortOrders(
            Dictionary<string, DarwinCoreTaxon> taxa,
            ZipArchive zipArchive,
            string csvFieldDelimiter)
        {
            // Try to get TaxonSortOrders.csv
            var taxonSortOrdersFile = zipArchive.Entries.FirstOrDefault(f =>
                f.Name.Equals("TaxonSortOrders.csv", StringComparison.CurrentCultureIgnoreCase));
            if (taxonSortOrdersFile == null) return; // If no taxon sort orders file found, we can't do anything more

            // Read taxon sort order data
            using var taxonSortOrdersReader = new StreamReader(taxonSortOrdersFile.Open(), Encoding.UTF8);
            using var taxonSortOrdersCsv = new CsvReader(taxonSortOrdersReader, GetCsvConfiguration(csvFieldDelimiter));

            // Get taxon sort orders by guid
            var allTaxonSortOrders = taxonSortOrdersCsv
                .GetRecords<TaxonSortOrder<string>>()
                .Select(m => new TaxonSortOrder<int>
                {
                    SortOrder = m.SortOrder,
                    TaxonId = GetTaxonIdfromDyntaxaGuid(m.TaxonId),                    
                });

            var taxonSortOrdersById = allTaxonSortOrders                
                .ToDictionary(g => g.TaxonId, g => g.SortOrder);

            foreach (var taxon in taxa.Values)
            {
                if (taxonSortOrdersById.TryGetValue(taxon.DynamicProperties.DyntaxaTaxonId, out var sortOrder))
                {
                    taxon.SortOrder = sortOrder.HasValue ? sortOrder.Value : 0;                    
                }
            }
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
            // Try to get VernacularName.csv
            var vernacularNameFile = zipArchive.Entries.FirstOrDefault(f =>
                f.Name.Equals("VernacularName.csv", StringComparison.CurrentCultureIgnoreCase));
            if (vernacularNameFile == null) return; // If no vernacular name file found, we can't do anything more

            // Read vernacular name data
            using var vernacularNameReader = new StreamReader(vernacularNameFile.Open(), Encoding.UTF8);
            using var vernacularNameCsv = new CsvReader(vernacularNameReader, GetCsvConfiguration(csvFieldDelimiter));

            // Get all vernacular names from file
            var vernacularNames = vernacularNameCsv.GetRecords<DarwinCoreVernacularName>().ToArray();
            var vernacularNamesByTaxonId = vernacularNames
                .GroupBy(m => m.TaxonID)
                .ToDictionary(g => g.Key, g => g.Select(m => m));

            foreach (var taxon in taxa.Values)
            {
                if (vernacularNamesByTaxonId.TryGetValue(taxon.TaxonID, out var dwcVernacularNames))
                {
                    taxon.VernacularNames = dwcVernacularNames;
                    taxon.VernacularName = dwcVernacularNames
                        .FirstOrDefault(m => m.Language == "sv" && m.IsPreferredName)?.VernacularName;
                }
            }
        }

        private class TaxonInfo
        {
            public int SortOrder { get; set; }
            public int Id { get; set; }
        }
    }
}