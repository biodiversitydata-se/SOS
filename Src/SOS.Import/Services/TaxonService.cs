using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CsvHelper;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Import;
using  SOS.Lib.Models.DarwinCore;
using SOS.Import.Mappings;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Models.TaxonTree;

namespace SOS.Import.Services
{
    public class TaxonService : Interfaces.ITaxonService
    {
        private const string DyntaxaTaxonIdPrefix = "urn:lsid:dyntaxa.se:Taxon:";
        private readonly string _taxonDwcUrl;
        private readonly ILogger<TaxonService> _logger;
        private readonly ITaxonServiceProxy _taxonServiceProxy;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxonServiceProxy"></param>
        /// <param name="taxonServiceConfiguration"></param>
        /// <param name="logger"></param>
        public TaxonService(
            ITaxonServiceProxy taxonServiceProxy, 
            TaxonServiceConfiguration taxonServiceConfiguration,
            ILogger<TaxonService> logger)
        {
            _taxonDwcUrl = taxonServiceConfiguration?.BaseAddress ?? throw new ArgumentNullException(nameof(taxonServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _taxonServiceProxy = taxonServiceProxy ?? throw new ArgumentNullException(nameof(taxonServiceProxy));
        }
        /// <inheritdoc />
        public async Task<IEnumerable<DarwinCoreTaxon>> GetTaxaAsync()
        {
            try
            {
                await using Stream zipFileContentStream = await _taxonServiceProxy.GetDwcaFileAsync(_taxonDwcUrl);
                if (zipFileContentStream == null) return null;
                using ZipArchive zipArchive = new ZipArchive(zipFileContentStream, ZipArchiveMode.Read, false);
                var csvFieldDelimiter = GetCsvFieldDelimiterFromMetaFile(zipArchive);
                Dictionary<string, DarwinCoreTaxon> taxa = GetTaxonCoreData(zipArchive, csvFieldDelimiter);
                AddVernacularNames(taxa, zipArchive, csvFieldDelimiter);
                AddTaxonRelations(taxa, zipArchive, csvFieldDelimiter);
                CalculateHigherClassificationField(taxa.Values.Select(m => m));
                return taxa.Values;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            return null;
        }

        /// <summary>
        /// Calculate higher classification tree by creating a taxon tree and iterate
        /// each nodes parents.
        /// </summary>
        /// <param name="taxa"></param>
        private void CalculateHigherClassificationField(IEnumerable<DarwinCoreTaxon> taxa)
        {
            var taxonById = taxa.ToDictionary(m => m.Id, m => m);
            var taxonTree = TaxonTreeFactory.CreateTaxonTree<DarwinCoreTaxon>(taxonById);
            foreach (var treeNode in taxonTree.TreeNodeById.Values)
            {
                var parentNames = treeNode.AsParentsNodeIterator().Select(m => m.ScientificName);
                var reversedParentNames = parentNames.Reverse();
                string higherClassification = string.Join(" | ", reversedParentNames);
                taxonById[treeNode.TaxonId].HigherClassification = higherClassification;
            }
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
                csvFieldDelimiter = xmlDoc.Root?.Element("archive")?.Element("core")?.Attribute("fieldsTerminatedBy")?.Value ??
                            csvFieldDelimiter;
            }

            return csvFieldDelimiter;
        }

        private static Dictionary<string, DarwinCoreTaxon> GetTaxonCoreData(ZipArchive zipArchive, string csvFieldDelimiter)
        {
            // Try to get the taxon data file
            var taxonFile = zipArchive.Entries.FirstOrDefault(f =>
                f.Name.Equals("Taxon.csv", StringComparison.CurrentCultureIgnoreCase));

            if (taxonFile == null) return null; // If no taxon data file found, we can't do anything more

            // Read taxon data
            using var taxonReader = new StreamReader(taxonFile.Open(), Encoding.UTF8);
            using var taxonCsv = new CsvReader(taxonReader, new CsvHelper.Configuration.Configuration
            {
                Delimiter = csvFieldDelimiter,
                Encoding = Encoding.UTF8,
                HasHeaderRecord = true
            });

            // Get all taxa from file
            taxonCsv.Configuration.RegisterClassMap<TaxonMapper>();
            var matchRegex = new Regex(DyntaxaTaxonIdPrefix + @"\d+$");
            var taxa = taxonCsv
                .GetRecords<DarwinCoreTaxon>()
                .Where(t => matchRegex.IsMatch(t.TaxonID))
                .ToDictionary(t => t.TaxonID, t => t);
            
            // Create initial Dynamic properties
            foreach (var taxon in taxa.Values)
            {
                taxon.DynamicProperties = new TaxonDynamicProperties();
                taxon.Id = taxon.DynamicProperties.DyntaxaTaxonId = GetTaxonIdfromDyntaxaGuid(taxon.TaxonID);
            }

            return taxa;
        }

        private static int GetTaxonIdfromDyntaxaGuid(string dyntaxaTaxonGuid)
        {
            return int.Parse(dyntaxaTaxonGuid.Substring(
                DyntaxaTaxonIdPrefix.Length,
                dyntaxaTaxonGuid.Length - DyntaxaTaxonIdPrefix.Length));
        }

        /// <summary>
        /// Reads taxon relations from zip file and add parent relations to taxa.
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
            using var taxonRelationsCsv = new CsvReader(taxonRelationsReader, new CsvHelper.Configuration.Configuration
            {
                Delimiter = csvFieldDelimiter,
                Encoding = Encoding.UTF8,
                HasHeaderRecord = true
            });

            taxonRelationsCsv.Configuration.RegisterClassMap<TaxonRelationMapper>();

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
                    taxon.DynamicProperties.ParentDyntaxaTaxonId = taxonRelations.FirstOrDefault(m => m.IsMainRelation)?.ParentTaxonId;
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
            using var vernacularNameCsv = new CsvReader(vernacularNameReader, new CsvHelper.Configuration.Configuration
            {
                Delimiter = csvFieldDelimiter,
                Encoding = Encoding.UTF8,
                HasHeaderRecord = true
            });

            // Get all vernacular names from file
            vernacularNameCsv.Configuration.RegisterClassMap<VernacularNameMapper>();
            var vernacularNames = vernacularNameCsv.GetRecords<DarwinCoreVernacularName>().ToArray();
            var vernacularNamesByTaxonId = vernacularNames
                .GroupBy(m => m.TaxonID)
                .ToDictionary(g => g.Key, g => g.Select(m => m));

            foreach (var taxon in taxa.Values)
            {
                if (vernacularNamesByTaxonId.TryGetValue(taxon.TaxonID, out var dwcVernacularNames))
                {
                    var taxonVernacularNames = dwcVernacularNames.ToTaxonVernacularNames();
                    taxon.DynamicProperties.VernacularNames = taxonVernacularNames;
                    taxon.VernacularName = taxonVernacularNames
                        .FirstOrDefault(m => m.Language == "sv" && m.IsPreferredName)?.Name;
                }
            }
        }
    }
}