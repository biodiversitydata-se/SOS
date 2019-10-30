using System;
using System.Collections.Generic;
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
using SOS.Lib.Configuration.Process;
using SOS.Lib.Models.DarwinCore;
using SOS.Process.Mappings;

namespace SOS.Process.Services
{
    public class TaxonService : Interfaces.ITaxonService
    {
        private readonly string _taxonDwcUrl;
        private readonly ILogger<TaxonService> _logger;

        public TaxonService(AppSettings settings, ILogger<TaxonService> logger)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _taxonDwcUrl = settings.TaxonDwcUrl;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        /// <inheritdoc />
        public async Task<IEnumerable<DarwinCoreTaxon>> GetTaxaAsync()
        {
            try
            {
                using var client = new HttpClient();
                using var result = await client.GetAsync(_taxonDwcUrl);

                if (!result.IsSuccessStatusCode)
                {
                    return null;
                }

                await using var zipFileContentStream = await result.Content.ReadAsStreamAsync();
                using var zipArchive = new ZipArchive(zipFileContentStream, ZipArchiveMode.Read, false);

                var delimiter = "\t";

                // try to get meta data file
                var metaFile = zipArchive.Entries.FirstOrDefault(f =>
                    f.Name.Equals("meta.xml", StringComparison.CurrentCultureIgnoreCase));

                // If we found the meta data file, try to get some settings from it
                if (metaFile != null)
                {
                    using var reader = XmlReader.Create(metaFile.Open());
                    var xmlDoc = XDocument.Load(reader);
                    delimiter = xmlDoc.Root?.Element("archive")?.Element("core")?.Attribute("fieldsTerminatedBy")?.Value ?? delimiter;
                }

                // Try to get the taxon data file
                var taxonFile = zipArchive.Entries.FirstOrDefault(f =>
                    f.Name.Equals("Taxon.csv", StringComparison.CurrentCultureIgnoreCase));

                // If no taxon data file found, we can't do anything more
                if (taxonFile == null)
                {
                    return null;
                }

                // Read taxon data
                using var taxonReader = new StreamReader(taxonFile.Open(), Encoding.UTF8);

                using var taxonCsv = new CsvReader(taxonReader, new CsvHelper.Configuration.Configuration
                {
                    Delimiter = delimiter,
                    Encoding = Encoding.UTF8,
                    HasHeaderRecord = true
                });

                // Get all taxa from file
                taxonCsv.Configuration.RegisterClassMap<TaxonMapper>();
                var matchRegex = new Regex(@"urn:lsid:dyntaxa.se:Taxon:\d+$");
                var taxa = taxonCsv.GetRecords<DarwinCoreTaxon>().Where(t => matchRegex.IsMatch(t.TaxonID)).ToDictionary(t => t.TaxonID, t => t);
                
                // Try to get the taxon data file
                var vernacularNameFile = zipArchive.Entries.FirstOrDefault(f =>
                    f.Name.Equals("VernacularName.csv", StringComparison.CurrentCultureIgnoreCase));

                // If no vernacular name file found, we can't do anything more
                if (vernacularNameFile == null)
                {
                    return taxa.Values;
                }

                // Read taxon data
                using var vernacularNameReader = new StreamReader(vernacularNameFile.Open(), Encoding.UTF8);
                using var vernacularNameCsv = new CsvReader(vernacularNameReader, new CsvHelper.Configuration.Configuration
                {
                    Delimiter = delimiter,
                    Encoding = Encoding.UTF8,
                    HasHeaderRecord = true
                });

                // Get all vernacular names from file
                vernacularNameCsv.Configuration.RegisterClassMap<VernacularNameMapper>();
                var vernacularNames = vernacularNameCsv.GetRecords<DarwinCoreVernacularName>().ToArray();

                foreach (var vernacularName in vernacularNames)
                {
                    // Try to get taxon
                    if (!taxa.TryGetValue(vernacularName.TaxonID, out var taxon))
                    {
                        continue;
                    }

                    // If vernacular not is set or if this is the preferred name
                    if (string.IsNullOrEmpty(taxon.VernacularName) || vernacularName.IsPreferredName)
                    {
                        taxon.VernacularName = vernacularName.VernacularName;
                    }
                }

                return taxa.Values;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            return null;
        }
    }
}
